export class ThemeToggle {
  
}

window.ThemeToggle = ThemeToggle;

export function toggle() {
    console.log("Toggle Theme Toggle");
    const html = document.documentElement;
    if (html.getAttribute('data-theme') === 'dark') {
        html.removeAttribute('data-theme');
    } else {
        html.setAttribute('data-theme', 'dark');
    }
}