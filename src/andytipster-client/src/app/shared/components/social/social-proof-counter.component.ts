import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface SocialProof {
  subscriberCount: number;
  tipsDelivered: number;
  winRate: number;
}

@Component({
  selector: 'app-social-proof-counter',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (proof) {
      <div class="stats stats-vertical lg:stats-horizontal shadow" role="group" aria-label="Performance statistics">
        <div class="stat">
          <div class="stat-title">Active Subscribers</div>
          <div class="stat-value text-primary">{{ proof.subscriberCount | number }}</div>
        </div>
        <div class="stat">
          <div class="stat-title">Tips Delivered</div>
          <div class="stat-value text-secondary">{{ proof.tipsDelivered | number }}</div>
        </div>
        <div class="stat">
          <div class="stat-title">Win Rate</div>
          <div class="stat-value text-accent">{{ proof.winRate }}%</div>
        </div>
      </div>
    }
  `
})
export class SocialProofCounterComponent implements OnInit {
  private readonly http = inject(HttpClient);
  proof: SocialProof | null = null;

  ngOnInit(): void {
    this.http.get<SocialProof>(`${environment.apiUrl}/api/social/proof`)
      .subscribe(data => this.proof = data);
  }
}
