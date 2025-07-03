import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: true,
  imports: [FormsModule, CommonModule, NgbToastModule],
})
export class SignupComponent {
  username: string = '';
  password: string = '';
  errorMessage: string = '';

  showEmptyFieldsToast = false;
  showInvalidCredsToast = false;
  showSuccessToast = false;
  showUsernameChangeToast = false;
  toastTimeout: any;

  constructor(private router: Router) {}

  signup() {
    if (!this.username || !this.password) {
      this.errorMessage = 'Username and password are required';
      this.showToast('emptyFieldsToast');
      return;
    }
    console.log('Attempting to log in with:', this.username, this.password);
    // Simulate a signup check
    fetch('https://localhost:7017/api/auth/register', {
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
          if (response.status === 409) {
            this.errorMessage = 'Username already exists';
            this.showToast('usernameChangeToast');
          } else {
            this.errorMessage = 'Network error or server is down';
          }
          throw new Error('Username already exists or network error');
        }
        return response.json();
      })
      .then((data) => {
        console.log('signup response:', data);
        if (data.success) {
          // Redirect to chat or dashboard
          this.showToast('successToast');
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 300);
          console.log('signup successful');
          this.errorMessage = '';
        } else {
          this.errorMessage = 'Invalid username or password';
        }
      })
      .catch((error) => {
        console.error('There was a problem with the signup request:', error);
        if (!this.errorMessage) {
          this.errorMessage = 'Signup failed. Please try again later.';
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
      case 'usernameChangeToast':
        this.showUsernameChangeToast = true;
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
