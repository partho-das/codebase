import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionType } from './ai-actor.service';
import {Centrifuge} from 'centrifuge';

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

  private centrifuge = new Centrifuge("ws://172.16.16.39:8000/connection/websocket", {
    token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJoc21hc3VkQG9yYml0YXguY29tIiwiZ2l2ZW5fbmFtZSI6Ikhhc2FuIiwiZmFtaWx5X25hbWUiOiJNYXN1ZCIsImVtYWlsIjoiaHNtYXN1ZEBvcmJpdGF4LmNvbSIsImNvbXBhbnlfaWQiOiJvcmJpdGF4IiwicHJvZHVjdF9jb2RlcyI6IlNVUEVSLVBPV0VSfENoYW5nZVJlcG9ydEF1dGhvcnxUYXhOZXdzQXV0aG9yfE9yYml0YXhTdXBwb3J0VGVhbXxXV01QRkFRIiwiaXNfY2hlY2tfcG9pbnRfdXNlciI6IkZhbHNlIiwianRpIjoiMzY4NzY2YWItMDdlZC00YjhkLWJlOTMtNTc0ZmI5NmY0ODU0IiwiU2Vzc2lvbklkIjoiNjgyRDFDMzMtMjQ5OC00Q0I0LUJEMzItNkU3MEM2NDQ3ODlEIiwibmJmIjoxNzA1MzA5MjY1LCJleHAiOjIyMjM3MDkyNjUsImlzcyI6ImxvY2FsaG9zdCJ9.VV6PNQJIF8q_EQZcGbxY6woqP7djQ_GI1iahmcZwL2g",
    debug: true
  });
  constructor(private http: HttpClient) {
    this.centrifuge.connect();
    this.centrifuge.on('connected', ctx => console.log('Connected to Centrifugo:', ctx));
    this.centrifuge.on('disconnected', ctx => console.log('Disconnected:', ctx));

    this.centrifuge.on('connected', (ctx) => console.log('Connected to Centrifugo', ctx));
    this.centrifuge.on('disconnected', ctx => console.log('Disconnected:', ctx));
    this.centrifuge.on("connecting", ctx => console.log('Connecting:', ctx));
    this.centrifuge.on("error", ctx => console.log('error:', ctx));
    this.centrifuge.on("message", ctx => console.log('Message:', ctx));



  }

  subscribe(channel: string, callback: (data: any) => void) {
    const sub = this.centrifuge.newSubscription(channel);
    sub.on('publication', ctx => callback(ctx.data));
    sub.on('subscribing', ctx => console.log('Subscribing:', ctx));
    sub.on('subscribed', ctx => console.log('subscribed:', ctx));
    sub.on('unsubscribed', ctx => console.log('unsubscribed:', ctx));
    sub.on('unsubscribed', ctx => console.log('unsubscribed:', ctx));
    sub.on('error', ctx => console.log('subscription error:', ctx));

    sub.subscribe();
  }

  chat(message: string): Observable<AiResponse> {
    return this.http.post<AiResponse>('/api/ai/agent', { message });
  }
}
