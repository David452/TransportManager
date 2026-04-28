using Core.Geocoding;

namespace Core.Trip.Optimiser;

public class NearestNeighbourOptimiser : ITripOptimiser
{
    
    public void Optimise(ref List<Order.Order> orders)
    {
        var stops = orders.SelectMany(o => new[] { o.Origin, o.Destination }).ToList();
        
        // matica vzdialenosti
        var matrix = new double[stops.Count, stops.Count];
        for (var i = 0; i < stops.Count; i++)
        {
            for (var j = 0; j < stops.Count; j++)
            {
                matrix[i, j] = GeoMath.HaversineDistanceKm(stops[i], stops[j]);
            }
        }
        
        // Referencia: https://developers.google.com/optimization/routing/pickup_delivery#c_1
        
        var sequence = NearestNeighbor(matrix, stops.Count);
        sequence = TwoOpt(sequence, matrix);

        orders = orders
            .Select((order, i) => (Order: order, Pos: sequence.IndexOf(2 * i)))
            .OrderBy(x => x.Pos)
            .Select(x => x.Order)
            .ToList();
    }

    // referencia: https://en.wikipedia.org/wiki/2-opt
    private static List<int> TwoOpt(List<int> sequence, double[,] matrix)
    {
        var best = new List<int>(sequence);
        var bestDist = CalculateTotalDistance(best, matrix);
        var improved = true;

        while (improved)
        {
            improved = false;
            for (var i = 0; i < best.Count - 1; i++)
            {
                for (var k = i + 1; k < best.Count; k++)
                {
                    var candidate = new List<int>(best);
                    candidate.Reverse(i + 1, k - i);

                    if (!IsPrecedenceValid(candidate)) continue;

                    var candidateDist = CalculateTotalDistance(candidate, matrix);
                    if (candidateDist < bestDist)
                    {
                        best = candidate;
                        bestDist = candidateDist;
                        improved = true;
                    }
                }
            }
        }

        return best;
    }

    private static List<int> NearestNeighbor(double[,] matrix, int stopCount)
    {
        var visited = new bool[stopCount];
        var sequence = new List<int>(stopCount);

        // 1. pickup
        visited[0] = true;
        sequence.Add(0);
        var current = 0;

        while (sequence.Count < stopCount)
        {
            var nearest = -1;
            var nearestDist = double.MaxValue;

            for (var j = 0; j < stopCount; j++)
            {
                if (visited[j]) continue;

                // Delivery (neparny index) je dostupnz az po jej pickupe (j-1)
                if (j % 2 != 0 && !visited[j - 1]) continue;

                var dist = matrix[current, j];
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = j;
                }
            }

            visited[nearest] = true;
            sequence.Add(nearest);
            current = nearest;
        }

        return sequence;
    }

    private static bool IsPrecedenceValid(IList<int> sequence)
    {
        for (var i = 0; i < sequence.Count; i++)
        {
            var stop = sequence[i];
            if (stop % 2 == 0) continue; // pickup, nie je čo validovať

            var pickupIndex = sequence.IndexOf(stop - 1);
            if (pickupIndex > i) return false;
        }
        return true;
    }

    private static double CalculateTotalDistance(IList<int> sequence, double[,] matrix)
    {
        var total = 0.0;
        for (var i = 0; i < sequence.Count - 1; i++)
            total += matrix[sequence[i], sequence[i + 1]];
        return total;
    }
}