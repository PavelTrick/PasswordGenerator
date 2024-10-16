import { Component, OnInit } from '@angular/core';
import { PasswordService, PasswordRequest } from '../../services/password.service';
import { Password } from '../../models/password.model';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { concatMap, finalize, of } from 'rxjs';
import { GenerateStatistic } from '../../models/generate-statistic.model';

@Component({
  selector: 'password-generator',
  templateUrl: './password-generator.component.html',
  styleUrls: ['./password-generator.component.scss']
})
export class PasswordGeneratorComponent implements OnInit {

  public useSimpleGenerator: boolean = true;

  passwordRequest: PasswordRequest = {
    amount: 1,
    length: 5,
    includeSpecial: true,
    includeNumbers: true,
    includeUppercase: true,
    includeLowercase: true,
    useSimpleGenerator: this.useSimpleGenerator,
  };

  public passwords: Password[] = [];
  public generateResult: any;
  public statistic: any;

  public generateLogs: GenerateStatistic[] = [];

  public generateInProgress: boolean = false;
  public deleteInProgress: boolean = false;
  public deleteAllInProgress: boolean = false;

  constructor(private passwordService: PasswordService, private authService: AuthService, private router: Router) { }

  ngOnInit() {
    this.loadUserPasswords();
    console.log('[ngOnInit]')
    setInterval(() => {
      this.loadUserPasswords();
    }, 60000)
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
    return this.deleteInProgress ? "Clearing..." : "Clear User Passwords";
  }

  get deleteAllBtnText(): string {
    return this.deleteAllInProgress ? "Clearing..." : "Clear Password Store";
  }

  onGenerate() {
    this.generateInProgress = true;
    this.passwordService.getNewPasswords(this.passwordRequest)
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

  generatePasswords() {
    console.log('[generatePasswords]')
    this.generateInProgress = true;
    this.passwordService.generatePassword(this.passwordRequest)
      .pipe(
        finalize(() => {
          this.generateInProgress = false;
        })
      )
      .subscribe(result => {
        this.loadUserPasswords();
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

  onClearAll() {
    this.deleteAllInProgress = true;
    this.passwordService.deleteStore()
      .pipe(
        finalize(() => {
          this.deleteAllInProgress = false;
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
    console.log('[loadUserPasswords]');
    this.passwordService.getUserPasswords().subscribe(result => {
        this.passwords = result ?? [];
    });
    
    this.passwordService.getPasswordStatistic().subscribe(statistic => {
      this.statistic = statistic;
    });

    this.passwordService.getLogs().subscribe(generateLogs => {
      this.generateLogs = generateLogs;
    });
  }
}