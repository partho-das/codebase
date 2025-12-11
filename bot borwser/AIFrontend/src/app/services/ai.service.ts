import { inject, Injectable, OnDestroy} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, SubscriptionLike } from 'rxjs';
import { ActionType } from './ai-actor.service';
import { NgZone } from '@angular/core';
import { EventSourceService } from './event-source.service';


export interface ActionCommand {
  type: ActionType;
  selector?: string;
  value?: string;
  durationMs?: number;
}


export enum ResponseType {
  Reasoning = 0, // e.g., AI explains or plans
 Thinking = 1, // e.g., partial streaming text
 NormalResponse = 2  
}

export interface AiResponse {
  ReplyText: string;
  Actions: ActionCommand[];
  Type: ResponseType
}

export interface AiRequest {
  Id : string;
  Message: string;
  SnapshotString?: string;
}

@Injectable({ providedIn: 'root' })
export class AiService implements OnDestroy{
   constructor(private http: HttpClient,  private eventSourceService: EventSourceService) {
 
   }
 

  chat(message: string): Observable<AiResponse> {
    return this.http.post<AiResponse>('/api/ai/chat', { message });
  }

  async sendMessage(message: string): Promise<string> {
    const response = await fetch('/api/ai/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message }),
    });

    const jsonResponse = await response.json();
    return jsonResponse.sessionId;
  }

  async startStream(message: string): Promise<Observable<AiResponse> | null> {
    const sessionId = await this.sendMessage(message);

    if (!sessionId) return null;

    const url = `api/ai/stream?sessionId=${sessionId}`;
    const options = { withCredentials: true };
    const eventNames = ['message'];

    return this.eventSourceService
        .connectToServerSentEvents(url, options, eventNames)
        .pipe(map((data: any) => data as AiResponse));
    }

     ngOnDestroy(): void {
    this.eventSourceService.close();
  }
}

  
