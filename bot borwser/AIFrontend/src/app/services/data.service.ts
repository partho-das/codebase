import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';


export interface CardData {
  id: number;
  title: string;
  description: string;
  image: string;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private baseUrl: string = "/api/data";
  constructor(private http: HttpClient) { }

  getCards(): Observable<CardData[]>{
    return this.http.get<CardData[]>(`${this.baseUrl}/cards`);
  }
  getCardDetails(id: number): Observable<CardData> {
    return this.http.get<CardData>(`${this.baseUrl}/cards/${id}`);
  }
}
