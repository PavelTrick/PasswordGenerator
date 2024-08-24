import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
    loginForm: FormGroup;

    constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
      console.log("LOGIN");
      this.loginForm = this.fb.group({
        login: ['', [Validators.required]],
        password: ['', Validators.required]
      });
    }
  
    onSubmit() {
        console.log("onSubmit");
      if (this.loginForm.valid) {
        const { login, password } = this.loginForm.value;
        this.authService.register(login, password).subscribe(
          response => {
            if(response) {
                this.router.navigate(['/home']);
            }
          },
          error => {
          }
        );
      }
    }
  }