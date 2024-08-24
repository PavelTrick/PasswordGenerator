import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    loginForm: FormGroup;

    constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
      console.log("LOGIN");
      this.loginForm = this.fb.group({
        login: ['', [Validators.required]],
        password: ['', Validators.required]
      });
    }
  
    onSubmit() {
      if (this.loginForm.valid) {
        const { login, password } = this.loginForm.value;
        this.authService.login(login, password).subscribe(
          response => {
            this.router.navigate(['/home']);
          },
          error => {
            // Handle login error
          }
        );
      }
    }

    onRegister() {
      this.router.navigate(['/register']);
    }
  }