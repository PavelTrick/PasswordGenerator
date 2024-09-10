import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { Password } from '../models/password.model';
import { PasswordRequest } from '../models/password-request.model';
import { GenerateResult } from '../models/generate-result.model';
import { Statistic } from '../models/statistic.model';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { BaseApiService } from './base-api.service';

@Injectable({
  providedIn: 'root'
})
export class PasswordService extends BaseApiService {
  constructor(private http: HttpClient, private router: Router, private authService: AuthService) {
    super();
   }

  generatePassword(request: PasswordRequest): Observable<GenerateResult> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post<GenerateResult>(`${this.baseUrl}/password`, request, { headers, withCredentials: true }).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status) {
          alert(`Backend returned error code ${error.status} ${error.error.title}. ${error.error.detail}.`);
        } else {
          alert('An unexpected error occurred. Please try again later.');
        }
        return of();
      })
    );
  }

  getUserPasswords(): Observable<Password[] | null> {
    return this.http.get<Password[]>(`${this.baseUrl}/password/list`, { withCredentials: true })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.log(error)
          if (error.status) {
            alert(`Backend returned error code ${error.status} ${error.error}`);
          } else {
            alert('An unexpected error occurred. Please try again later.');
          }
          if(error.status === 400) {
            this.authService.logout();
            localStorage.removeItem('authToken');
            this.router.navigate(['/login']);
          }
          return of(null);
        })
      );
  }

  delete(): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseUrl}/password`, { withCredentials: true });
  }

  getUserPasswordStatistic(): Observable<Statistic> {
    return this.http.get<Statistic>(`${this.baseUrl}/password/statistic`, { withCredentials: true })
    .pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status) {
          alert(`Backend returned error code ${error.status} ${error.error}`);
        } else {
          alert('An unexpected error occurred. Please try again later.');
        }

        if(error.status === 400) {
          this.authService.logout();
          localStorage.removeItem('authToken');
          this.router.navigate(['/login']);
        }

        return of();
      })
    );;
  }
}

export { PasswordRequest };
