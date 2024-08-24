import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { Password } from '../models/password.model';
import { PasswordRequest } from '../models/password-request.model';
import { GenerateResult } from '../models/generate-result.model';
import { Statistic } from '../models/statistic.model';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class PasswordService {
  private apiUrl = 'https://localhost:5091/api/password';

  constructor(private http: HttpClient, private router: Router, private authService: AuthService) { }

  generatePassword(request: PasswordRequest): Observable<GenerateResult> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post<GenerateResult>(this.apiUrl, request, { headers, withCredentials: true }).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status) {
          alert(`Backend returned error code ${error.status} ${error.error.title}`);
        } else {
          alert('An unexpected error occurred. Please try again later.');
        }
        return of();
      })
    );
  }

  getAll(): Observable<Password[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' })
    return this.http.get<Password[]>(this.apiUrl, { headers, withCredentials: true });
  }

  getUserPasswords(): Observable<Password[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.get<Password[]>(`${this.apiUrl}/list`, { headers, withCredentials: true })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status) {
            alert(`Backend returned error code ${error.status} ${error.error.title}`);
          } else {
            alert('An unexpected error occurred. Please try again later.');
          }

          if(error.status === 400) {
            this.authService.logout();
            localStorage.removeItem('authToken');
            this.router.navigate(['/login']);
          }

          return of([]);
        })
      );
  }

  delete(userId: number): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' })
    return this.http.delete<boolean>(`${this.apiUrl}`, { headers, withCredentials: true });
  }

  getUserPasswordStatistic(userId: number): Observable<Statistic> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' })
    return this.http.get<Statistic>(`${this.apiUrl}/statistic`, { headers, withCredentials: true })
    .pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status) {
          alert(`Backend returned error code ${error.status} ${error.error.title}`);
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
