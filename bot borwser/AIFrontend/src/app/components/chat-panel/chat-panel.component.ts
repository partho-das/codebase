import { Component, OnInit } from '@angular/core';
import {ActionCommand, AiResponse, AiService} from '../../services/ai.service';
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
export class ChatPanelComponent implements OnInit {
  messages: { sender: 'user' | 'ai'; text: string }[] = [];
  input = '';

  constructor(private ai: AiService, private actor: AiActorService, private uiCapService: UiCaptureService) {}

  ngOnInit() {
    this.actor.initCursor();
    this.ai.subscribe('partho_ai_chat', (res: any) => {
      let data : AiResponse = JSON.parse(res);
      console.log("Socket Data:"  + JSON.stringify(data));
      this.messages.push({sender: 'ai', text: data.replyText});
      this.actor.runActions(data.actions);
    });
  }

  send() {
    const msg = this.input.trim();
    if (!msg) return;
    this.messages.push({ sender: 'user', text: msg });
    this.input = '';

    this.uiCapService.requestAiActon(msg).subscribe({
      next: (res) => {console.log("https Response: " + res)}
    });
  }
}
