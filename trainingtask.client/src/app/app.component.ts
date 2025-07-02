import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Chat Application';
  showNav = true;

  constructor(private router: Router) {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(event => {
        // Add all routes where nav should be hidden
        const hiddenRoutes = ['/login'];
        this.showNav = !hiddenRoutes.includes(event.urlAfterRedirects);
      });
  }

  onLogout() {
    fetch('https://localhost:7017/api/auth/logout', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include' // Include cookies for session management
    })
    .then(response => {
      if (response.ok) {
        this.router.navigate(['/login']);
      }
    });
  }
}
