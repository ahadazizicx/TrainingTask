import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule, NgbToastModule],
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  errorMessage: string = '';

  showEmptyFieldsToast = false;
  showInvalidCredsToast = false;
  showSuccessToast = false;
  toastTimeout: any;

  constructor(private router: Router) {}

  login() {
    if (!this.username || !this.password) {
      this.errorMessage = 'Username and password are required';
      this.showToast('emptyFieldsToast');
      return;
    }
    console.log('Attempting to log in with:', this.username, this.password);
    fetch('https://localhost:7017/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({
        username: this.username,
        password: this.password,
      }),
    })
      .then((response) => {
        if (!response.ok) {
          this.showToast('invalidCredsToast');
          this.errorMessage = 'Invalid username or password';
          throw new Error('Invalid credentials or network error');
        }
        return response.json();
      })
      .then((data) => {
        console.log('Login response:', data);
        if (data.success) {
          this.showToast('successToast');
          setTimeout(() => {
            this.router.navigate(['/config']);
          }, 300);
          console.log('Login successful');
          this.errorMessage = '';
        } else {
          this.showToast('invalidCredsToast');
          this.errorMessage = 'Invalid username or password';
        }
      })
      .catch((error) => {
        console.error('There was a problem with the login request:', error);
        if (!this.errorMessage) {
          this.errorMessage = 'Login failed. Please try again later.';
        }
      });
  }

  showToast(type: string) {
    // Hide all toasts first
    this.showSuccessToast = false;
    this.showEmptyFieldsToast = false;
    this.showInvalidCredsToast = false;
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }
    switch (type) {
      case 'emptyFieldsToast':
        this.showEmptyFieldsToast = true;
        break;
      case 'successToast':
        this.showSuccessToast = true;
        break;
      case 'invalidCredsToast':
        this.showInvalidCredsToast = true;
        break;
    }
    // Hide toast after 2 seconds
    this.toastTimeout = setTimeout(() => {
      this.showEmptyFieldsToast = false;
      this.showInvalidCredsToast = false;
      this.showSuccessToast = false;
    }, 2000);
  }
}
