import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AiResponse, AiService, ResponseType } from '../../services/ai.service';
import { AiActorService } from '../../services/ai-actor.service';
import {FormsModule} from '@angular/forms';
import {NgForOf} from '@angular/common';
import { UiCaptureService } from '../../services/ui-capture.service';

@Component({
  selector: 'app-chat-panel',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf
  ],
  templateUrl: './chat-panel.component.html'
})
export class ChatPanelComponent{
  @ViewChild('chatWindow') chatWindow!: ElementRef;

  userMessages: { text: string }[] = [];
  reasoningMessages: { text: string }[] = [];
  normalMessages: { text: string }[] = [];
  input = '';

  constructor(private aiService: AiService) {}

  async send() {
    const msg = this.input.trim();
    if (!msg) return;

    this.userMessages.push({ text: msg });
    this.input = '';

     const stream$ = await this.aiService.startStream(msg);
  if (!stream$) return;

    stream$.subscribe({
      next: (chunk: AiResponse) => {
        if (chunk.Type == ResponseType.Reasoning) {
          this.reasoningMessages.push({ text: chunk.ReplyText });
        } else {
          this.normalMessages.push({ text: chunk.ReplyText });
        }

        this.scrollToBottom();
      },
      error: (err) => {
        this.normalMessages.push({ text: 'Error: ' + err.message });
        this.scrollToBottom();
      },
    });
  }

  private scrollToBottom() {
    setTimeout(() => {
      if (this.chatWindow) {
        const el = this.chatWindow.nativeElement;
        el.scrollTop = el.scrollHeight;
      }
    }, 50);
  }
}
