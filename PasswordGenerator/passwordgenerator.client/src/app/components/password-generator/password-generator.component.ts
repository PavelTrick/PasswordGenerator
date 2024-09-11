import { Component, OnInit } from '@angular/core';
import { PasswordService, PasswordRequest } from '../../services/password.service';
import { Password } from '../../models/password.model';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { concatMap, finalize, of } from 'rxjs';

@Component({
  selector: 'password-generator',
  templateUrl: './password-generator.component.html',
  styleUrls: ['./password-generator.component.scss']
})
export class PasswordGeneratorComponent implements OnInit {

  public useSimpleGenerator: boolean = false;

  passwordRequest: PasswordRequest = {
    amount: 1,
    length: 5,
    includeSpecial: false,
    includeNumbers: false,
    includeUppercase: false,
    includeLowercase: true,
    useSimpleGenerator: this.useSimpleGenerator,
  };

  public passwords: Password[] = [];
  public generateResult: any;
  public statistic: any;

  public generateInProgress: boolean = false;
  public deleteInProgress: boolean = false;

  constructor(private passwordService: PasswordService, private authService: AuthService, private router: Router) { }

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

  get deleteBtnText(): string {
    return this.deleteInProgress ? "Clearing..." : "Clear all";
  }

  onGenerate() {
    this.generateInProgress = true;
    this.passwordService.generatePassword(this.passwordRequest)
      .pipe(
        finalize(() => {
          this.generateInProgress = false;
        })
      )
      .subscribe(result => {
        if (result) {
          this.generateResult = result;
          this.loadUserPasswords();
        }
      });
  }

  onClear() {
    this.deleteInProgress = true;
    this.passwordService.delete()
      .pipe(
        finalize(() => {
          this.deleteInProgress = false;
        })
      )
      .subscribe(_ => {
        this.loadUserPasswords();
        this.generateResult = null;
      });
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private loadUserPasswords() {
    this.passwordService.getUserPasswords().pipe(
      concatMap(result => {
        if (result === null) {
          this.passwords = [];
          return of(null);
        }

        this.passwords = result;
        return this.passwordService.getUserPasswordStatistic();
      })
    ).subscribe(statistic => {
      this.statistic = statistic;
    });
  }
}