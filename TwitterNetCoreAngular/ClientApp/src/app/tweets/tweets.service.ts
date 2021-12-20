import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TweetsService {
  baseUrl = '';
  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  getAll(): Promise<any> {
    return this.http.get(this.baseUrl + 'tweets').toPromise();
  }
}
