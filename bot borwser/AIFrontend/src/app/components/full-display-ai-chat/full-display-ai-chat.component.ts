import { Component, ElementRef, ViewChild } from '@angular/core';
import { AiService, AiResponse, ResponseType } from '../../services/ai.service';
import { FormsModule } from '@angular/forms';
import { CommonModule, NgForOf } from '@angular/common';


interface AiMessage{
  reasoningText : string;
  normalText : string;
  userInput: string;
}
@Component({
  selector: 'app-full-display-ai-chat',
  standalone: true,
  imports: [  CommonModule, FormsModule, NgForOf],
  templateUrl: './full-display-ai-chat.component.html',
  styleUrl: './full-display-ai-chat.component.scss'
})
export class FullDisplayAiChatComponent {
  aiMessages: AiMessage[] = [];
  input = '';

  constructor(private aiService: AiService) {}

  async send() {
    const msg = this.input.trim();
    if (!msg) return;
    let aiResponse: AiMessage = {reasoningText: "", normalText: "", userInput: msg};

    this.input = '';

    const stream$ = await this.aiService.startStream(msg);
    if (!stream$) return;
    
    this.aiMessages.push(aiResponse);

    stream$.subscribe({
      next: (chunk: AiResponse) => {
        if (chunk.Type == ResponseType.Reasoning) {
          aiResponse.reasoningText += chunk.ReplyText;
        } else {
          aiResponse.normalText += chunk.ReplyText;
        }
        this.aiMessages.pop();
        this.aiMessages.push(aiResponse);
        if(aiResponse.normalText){
          console.log("HI");
        }
        if(aiResponse.normalText.trim()){
             console.log("HI");
        }
         if(aiResponse.normalText.length > 0){
          console.log("HI");
        }

      },
      error: (err) => {
        this.aiMessages.push({ userInput: msg,  reasoningText: "Error: By Stestem", normalText: 'Error: ' + err.message });
      },
    });
  }
}
