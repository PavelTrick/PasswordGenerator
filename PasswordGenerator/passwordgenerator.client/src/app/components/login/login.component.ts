import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    loginForm: FormGroup;

    constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
      this.loginForm = this.fb.group({
        login: ['', [Validators.required]],
        password: ['', Validators.required]
      });
    }
  
    onSubmit() {
      const { login, password } = this.loginForm.value;
      this.authService.test(login, password).subscribe(
        response => {
          console.log(response)
        },
        (error: HttpErrorResponse) => {
          if (error instanceof HttpErrorResponse) {
            if (error.status) {
              alert(`Backend returned code ${error.status} ${error.statusText}`);
            }
          } else {
            alert('An unexpected error occurred. Please try again later.');
          }
        });


      if (this.loginForm.valid) {
        const { login, password } = this.loginForm.value;
        this.authService.login(login, password).subscribe(
          response => {
            this.router.navigate(['/home']);
          },
          (error: HttpErrorResponse) => {
            if (error instanceof HttpErrorResponse) {
              if (error.status) {
                alert(`Backend returned code ${error.status} ${error.statusText}`);
              }
            } else {
              alert('An unexpected error occurred. Please try again later.');
            }
          });
      }
    }

    onRegister() {
      this.router.navigate(['/register']);
    }
  }