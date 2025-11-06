import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionType } from './ai-actor.service';

export interface ActionCommand {
  type: ActionType;
  selector?: string;
  value?: string;
  durationMs?: number;
}

export interface AiResponse {
  replyText: string;
  actions: ActionCommand[];
}

@Injectable({ providedIn: 'root' })
export class AiService {
  constructor(private http: HttpClient) {}

  chat(message: string): Observable<AiResponse> {
    return this.http.post<AiResponse>('/api/ai/agent', { message });
  }
}
