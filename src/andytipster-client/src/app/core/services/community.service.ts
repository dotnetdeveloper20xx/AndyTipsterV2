import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CommentDto {
  id: string;
  userId: string;
  authorName: string;
  authorAvatarUrl?: string;
  tipId: string;
  content: string;
  isApproved: boolean;
  createdAt: string;
}

export interface CreateCommentDto {
  tipId: string;
  content: string;
}

export interface PollDto {
  id: string;
  question: string;
  options: PollOptionDto[];
  totalVotes: number;
  isActive: boolean;
  createdAt: string;
  closedAt?: string;
  userVotedOptionId?: string;
}

export interface PollOptionDto {
  id: string;
  text: string;
  voteCount: number;
  percentage: number;
}

export interface CreatePollDto {
  question: string;
  options: string[];
}

export interface VoteDto {
  pollId: string;
  optionId: string;
}

export interface MessageDto {
  id: string;
  senderId: string;
  senderName: string;
  recipientId: string;
  recipientName: string;
  content: string;
  isRead: boolean;
  createdAt: string;
}

export interface SendMessageDto {
  recipientId: string;
  content: string;
}

export interface ConversationDto {
  participantId: string;
  participantName: string;
  participantAvatarUrl?: string;
  lastMessage: string;
  lastMessageAt: string;
  unreadCount: number;
}

@Injectable({ providedIn: 'root' })
export class CommunityService {
  private readonly http = inject(HttpClient);
  private readonly commentsUrl = `${environment.apiUrl}/api/comments`;
  private readonly pollsUrl = `${environment.apiUrl}/api/polls`;
  private readonly messagesUrl = `${environment.apiUrl}/api/messages`;

  // === Comments ===

  getCommentsForTip(tipId: string, page = 1, pageSize = 20): Observable<CommentDto[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<CommentDto[]>(`${this.commentsUrl}/tip/${tipId}`, { params });
  }

  createComment(dto: CreateCommentDto): Observable<CommentDto> {
    return this.http.post<CommentDto>(this.commentsUrl, dto);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.commentsUrl}/${commentId}`);
  }

  hideComment(commentId: string): Observable<void> {
    return this.http.post<void>(`${this.commentsUrl}/${commentId}/hide`, {});
  }

  // === Polls ===

  getActivePolls(): Observable<PollDto[]> {
    return this.http.get<PollDto[]>(this.pollsUrl);
  }

  getPoll(pollId: string): Observable<PollDto> {
    return this.http.get<PollDto>(`${this.pollsUrl}/${pollId}`);
  }

  createPoll(dto: CreatePollDto): Observable<PollDto> {
    return this.http.post<PollDto>(this.pollsUrl, dto);
  }

  vote(pollId: string, optionId: string): Observable<PollDto> {
    return this.http.post<PollDto>(`${this.pollsUrl}/${pollId}/vote`, { optionId });
  }

  closePoll(pollId: string): Observable<void> {
    return this.http.post<void>(`${this.pollsUrl}/${pollId}/close`, {});
  }

  // === Messages ===

  getConversations(): Observable<ConversationDto[]> {
    return this.http.get<ConversationDto[]>(`${this.messagesUrl}/conversations`);
  }

  getConversation(participantId: string, page = 1, pageSize = 20): Observable<MessageDto[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<MessageDto[]>(`${this.messagesUrl}/conversation/${participantId}`, { params });
  }

  sendMessage(dto: SendMessageDto): Observable<MessageDto> {
    return this.http.post<MessageDto>(this.messagesUrl, dto);
  }

  markConversationAsRead(participantId: string): Observable<void> {
    return this.http.post<void>(`${this.messagesUrl}/conversation/${participantId}/read`, {});
  }
}
