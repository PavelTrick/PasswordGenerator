import { Component, OnInit } from '@angular/core';
import { PasswordService, PasswordRequest } from '../../services/password.service';
import { Password } from '../../models/password.model';
import { GenerateResult } from '../../models/generate-result.model';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'password-generator',
  templateUrl: './password-generator.component.html',
  styleUrls: ['./password-generator.component.scss']
})
export class PasswordGeneratorComponent implements OnInit {
  passwordRequest: PasswordRequest = {
    amount: 1,
    length: 5,
    includeSpecial: false,
    includeNumbers: false,
    includeUppercase: false,
    includeLowercase: true,
  };

  private userId: number = 1;
  public passwords: Password[] = [];
  public generateResult: any;
  public statistic: any;

  public generateInProgress: boolean = false;

  constructor(private passwordService: PasswordService, private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.loadUserPasswords();
  }

  get formattedPasswords(): string {
    return this.passwords
      .map((password) => `${password.code}`)
      .join('\n');
  }

  get generateBtnText(): string {
    return this.generateInProgress ? "Loading..." : "Generate";
  }

  onGenerate() {
    this.generateInProgress = true;
    this.passwordService.generatePassword(this.passwordRequest)
      .subscribe(result => {
        this.generateInProgress = false;

        if(result) {
          this.generateResult = result;
          this.loadUserPasswords();
        }
      });
  }

  onClear() {
    this.passwordService.delete(this.userId)
      .subscribe(_ => {
       this.loadUserPasswords();
      });
  }

  onLogout() {
      this.authService.logout();
      this.router.navigate(['/login']);
  }

  private loadUserPasswords() {
    this.passwordService.getUserPasswords().subscribe(result => {
      
      console.log(result);
      this.passwords = result;
    });

    this.passwordService.getUserPasswordStatistic(this.userId).subscribe(result => {
      this.statistic = result;
    });
  }
}