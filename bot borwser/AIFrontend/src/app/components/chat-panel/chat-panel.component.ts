import { Component, OnInit } from '@angular/core';
import { AiService } from '../../services/ai.service';
import { AiActorService } from '../../services/ai-actor.service';
import {FormsModule} from '@angular/forms';
import {NgForOf} from '@angular/common';

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

  constructor(private ai: AiService, private actor: AiActorService) {}

  ngOnInit() {
    this.actor.initCursor();
  }

  send() {
    const msg = this.input.trim();
    if (!msg) return;
    this.messages.push({ sender: 'user', text: msg });
    this.input = '';

    this.ai.chat(msg).subscribe({
      next: (res) => {
        this.messages.push({ sender: 'ai', text: res.replyText });
        this.actor.runActions(res.actions);
      },
      error: (err) => {
        this.messages.push({ sender: 'ai', text: 'Error: ' + err.message });
      },
    });
  }
}
