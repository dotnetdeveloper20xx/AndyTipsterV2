import { Component, inject, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface ChatMessage {
  id: string;
  sender: string;
  content: string;
  quickReplies?: string[];
  timestamp: string;
}

interface BotResponse {
  sessionId: string;
  message: string;
  quickReplies?: string[];
  isEscalated: boolean;
}

@Component({
  selector: 'app-help-bot-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Floating toggle button -->
    @if (!isOpen()) {
      <button
        class="fixed bottom-6 right-6 btn btn-circle btn-primary btn-lg shadow-xl z-50 transition-transform duration-300 hover:scale-110"
        (click)="open()"
        aria-label="Open help chat">
        💬
      </button>
    }

    <!-- Chat widget -->
    @if (isOpen()) {
      <div class="fixed bottom-6 right-6 w-80 max-h-[500px] bg-base-100 border border-base-300 rounded-xl shadow-2xl z-50 flex flex-col transition-all duration-300" role="dialog" aria-label="Help chat">
        <!-- Header -->
        <div class="bg-primary text-primary-content p-3 rounded-t-xl flex justify-between items-center">
          <span class="font-semibold text-sm">Help & Support</span>
          <button class="btn btn-ghost btn-xs btn-circle" (click)="close()" aria-label="Close chat">✕</button>
        </div>

        <!-- Messages area -->
        <div class="flex-1 overflow-y-auto p-3 space-y-3 min-h-[250px] max-h-[350px]">
          @for (msg of messages(); track msg.id) {
            <div [class]="msg.sender === 'bot' ? 'chat chat-start' : 'chat chat-end'">
              <div [class]="msg.sender === 'bot' ? 'chat-bubble chat-bubble-primary text-sm' : 'chat-bubble text-sm'">
                {{ msg.content }}
              </div>
            </div>
            @if (msg.quickReplies && msg.quickReplies.length > 0) {
              <div class="flex flex-wrap gap-1 mt-1">
                @for (reply of msg.quickReplies; track reply) {
                  <button
                    class="btn btn-xs btn-outline"
                    (click)="sendQuickReply(reply)">
                    {{ reply }}
                  </button>
                }
              </div>
            }
          }
        </div>

        <!-- Input area -->
        <div class="p-3 border-t border-base-300">
          <div class="flex gap-2">
            <input
              type="text"
              class="input input-sm input-bordered flex-1"
              placeholder="Type a message..."
              [(ngModel)]="userMessage"
              (keyup.enter)="send()"
              aria-label="Type your message" />
            <button
              class="btn btn-sm btn-primary"
              (click)="send()"
              [disabled]="!userMessage.trim()"
              aria-label="Send message">
              ➤
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class HelpBotWidgetComponent {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/helpbot`;

  isOpen = signal(false);
  messages = signal<ChatMessage[]>([]);
  userMessage = '';
  private sessionId: string | null = null;

  open(): void {
    this.isOpen.set(true);
    if (this.messages().length === 0) {
      this.initConversation();
    }
  }

  close(): void {
    this.isOpen.set(false);
  }

  send(): void {
    const msg = this.userMessage.trim();
    if (!msg) return;

    this.addMessage('user', msg);
    this.userMessage = '';
    this.sendToBot(msg);
  }

  sendQuickReply(reply: string): void {
    this.addMessage('user', reply);
    this.sendToBot(reply);
  }

  private initConversation(): void {
    this.http.post<BotResponse>(`${this.apiUrl}/message`, { message: '', sessionId: null })
      .subscribe(response => {
        this.sessionId = response.sessionId;
        this.addMessage('bot', response.message, response.quickReplies);
      });
  }

  private sendToBot(message: string): void {
    this.http.post<BotResponse>(`${this.apiUrl}/message`, { message, sessionId: this.sessionId })
      .subscribe(response => {
        this.sessionId = response.sessionId;
        this.addMessage('bot', response.message, response.quickReplies);
      });
  }

  private addMessage(sender: string, content: string, quickReplies?: string[]): void {
    const newMsg: ChatMessage = {
      id: crypto.randomUUID(),
      sender,
      content,
      quickReplies,
      timestamp: new Date().toISOString()
    };
    this.messages.update(msgs => [...msgs, newMsg]);
  }
}
