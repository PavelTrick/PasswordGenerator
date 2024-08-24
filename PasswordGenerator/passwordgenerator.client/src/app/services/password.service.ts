import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Password } from '../models/password.model';
import { PasswordRequest } from '../models/password-request.model';
import { GenerateResult } from '../models/generate-result.model';
import { Statistic } from '../models/statistic.model';

@Injectable({
  providedIn: 'root'
})
export class PasswordService {
  private apiUrl = 'http://localhost:5091/api/password';

  constructor(private http: HttpClient) { }

  generatePassword(request: PasswordRequest): Observable<GenerateResult> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post<GenerateResult>(this.apiUrl, request, { headers, withCredentials: true });
  }

  getAll(): Observable<Password[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' })
    return this.http.get<Password[]>(this.apiUrl, { headers, withCredentials: true });
  }

  getUserPasswords(): Observable<Password[]> {
    const token = localStorage.getItem('authToken');
    return this.http.get<Password[]>(`${this.apiUrl}/list`, { withCredentials: true });
  }

  delete(userId: number): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' })
    return this.http.delete<boolean>(`${this.apiUrl}`, { headers, withCredentials: true });
  }

  getUserPasswordStatistic(userId: number): Observable<Statistic> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' })
    return this.http.get<Statistic>(`${this.apiUrl}/statistic`, { headers, withCredentials: true });
  }
}

export { PasswordRequest };
