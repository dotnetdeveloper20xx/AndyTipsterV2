import { ChangeDetectionStrategy, Component, OnInit, signal, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PlansService } from '../../../../core/services/plans.service';
import { Plan } from '../../../../store/plans/plans.state';

interface Testimonial {
  name: string;
  quote: string;
  rating: number;
}

interface FaqItem {
  question: string;
  answer: string;
  open: boolean;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Hero Section -->
    <section class="hero min-h-[80vh] relative overflow-hidden bg-gradient-to-br from-primary to-secondary" [class.parallax-active]="scrollY() > 100">
      <div class="absolute inset-0 bg-[url('/assets/hero-pattern.svg')] opacity-5"></div>
      <div class="hero-content text-center relative z-10" [style.transform]="'translateY(' + scrollY() * 0.3 + 'px)'">
        <div class="max-w-2xl animate-fade-in">
          <h1 class="text-5xl md:text-6xl font-bold text-primary-content mb-4">
            AndyTipster
          </h1>
          <p class="text-xl md:text-2xl text-primary-content/90 mb-2">
            UK & Ireland Horse Racing Tips
          </p>
          <p class="text-lg text-primary-content/70 mb-8">
            Expert selections backed by data-driven analysis. Join thousands of profitable punters.
          </p>
          <a routerLink="/auth/register" class="btn btn-warning btn-lg shadow-lg hover:scale-105 transition-transform text-warning-content">
            Start Winning Today
          </a>
          <p class="text-sm text-primary-content/50 mt-4">No commitment required. Cancel anytime.</p>
        </div>
      </div>
    </section>

    <!-- Stats Section -->
    <section class="py-16 bg-base-200" [class.animate-slide-up]="scrollY() > 200">
      <div class="container mx-auto px-4">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
          <div class="stat place-items-center bg-base-200 rounded-box">
            <div class="stat-value text-warning">87%</div>
            <div class="stat-desc">Strike Rate</div>
          </div>
          <div class="stat place-items-center bg-base-200 rounded-box">
            <div class="stat-value text-warning">£2,340</div>
            <div class="stat-desc">Avg Monthly Profit</div>
          </div>
          <div class="stat place-items-center bg-base-200 rounded-box">
            <div class="stat-value text-warning">1,200+</div>
            <div class="stat-desc">Active Subscribers</div>
          </div>
          <div class="stat place-items-center bg-base-200 rounded-box">
            <div class="stat-value text-warning">5 yrs</div>
            <div class="stat-desc">Verified Track Record</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Pricing Table -->
    <section class="py-20 bg-base-100" id="pricing">
      <div class="container mx-auto px-4">
        <h2 class="text-3xl font-bold text-center mb-3">Choose Your Plan</h2>
        <p class="text-center text-base-content/60 mb-12">All plans include our money-back guarantee</p>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-5xl mx-auto">
          @for (plan of plans(); track plan.id; let i = $index) {
            <div
              class="card bg-base-100 border-2 hover:shadow-xl transition-all duration-300"
              [class.border-primary]="i === 1"
              [class.scale-105]="i === 1"
              [class.border-base-300]="i !== 1"
              [style.animation-delay]="i * 100 + 'ms'"
            >
              @if (i === 1) {
                <div class="bg-primary text-primary-content text-center text-xs py-1 font-medium">Most Popular</div>
              }
              <div class="card-body text-center">
                <h3 class="text-xl font-bold">{{ plan.name }}</h3>
                <div class="py-4">
                  <span class="text-4xl font-bold">{{ plan.currency === 'GBP' ? '£' : plan.currency === 'EUR' ? '€' : '$' }}{{ plan.price }}</span>
                  <span class="text-base-content/60">/{{ plan.billingCycle | lowercase }}</span>
                </div>
                <ul class="text-sm text-left space-y-2 mb-6">
                  @for (feature of plan.features; track feature) {
                    <li class="flex items-center gap-2">
                      <span class="text-success">✓</span>
                      {{ feature }}
                    </li>
                  }
                </ul>
                <a
                  routerLink="/auth/register"
                  class="btn w-full"
                  [class.btn-primary]="i === 1"
                  [class.btn-outline]="i !== 1"
                >
                  Get Started
                </a>
              </div>
            </div>
          }
        </div>
      </div>
    </section>

    <!-- Tip of the Day Preview -->
    <section class="py-16 bg-base-200">
      <div class="container mx-auto px-4 max-w-2xl">
        <h2 class="text-3xl font-bold text-center mb-8">Today's Free Tip</h2>
        <div class="card bg-base-100 shadow-lg">
          <div class="card-body">
            <div class="flex items-center gap-3 mb-3">
              <span class="badge badge-primary">🏇 Free Preview</span>
              <span class="text-sm text-base-content/60">{{ today | date:'mediumDate' }}</span>
            </div>
            <h3 class="text-lg font-semibold">Cheltenham - 2:30 Race</h3>
            <p class="text-base-content/70">Our expert pick for today. Subscribe to see the full selection, odds, and stake recommendation.</p>
            <div class="mt-4 p-3 bg-base-200 rounded-lg text-center">
              <p class="text-sm text-base-content/60 mb-2">🔒 Full tip details are for subscribers only</p>
              <a routerLink="/auth/register" class="btn btn-sm btn-primary">Subscribe to Unlock</a>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Testimonials Carousel -->
    <section class="py-20 bg-base-100">
      <div class="container mx-auto px-4">
        <h2 class="text-3xl font-bold text-center mb-12">What Our Members Say</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-5xl mx-auto">
          @for (testimonial of visibleTestimonials(); track testimonial.name) {
            <div class="card bg-base-200 shadow-sm">
              <div class="card-body">
                <div class="flex mb-2">
                  @for (star of [1,2,3,4,5]; track star) {
                    <span [class.text-warning]="star <= testimonial.rating" [class.text-base-content/20]="star > testimonial.rating">★</span>
                  }
                </div>
                <p class="italic text-base-content/80">"{{ testimonial.quote }}"</p>
                <p class="font-semibold mt-3 text-sm">— {{ testimonial.name }}</p>
              </div>
            </div>
          }
        </div>
        <div class="flex justify-center mt-6 gap-2">
          @for (dot of [0, 1, 2]; track dot) {
            <button
              class="w-3 h-3 rounded-full transition-colors"
              [class.bg-primary]="testimonialPage() === dot"
              [class.bg-base-300]="testimonialPage() !== dot"
              (click)="testimonialPage.set(dot)"
              [attr.aria-label]="'Testimonial page ' + (dot + 1)"
            ></button>
          }
        </div>
      </div>
    </section>

    <!-- FAQ Section -->
    <section class="py-20 bg-base-200" id="faq">
      <div class="container mx-auto px-4 max-w-3xl">
        <h2 class="text-3xl font-bold text-center mb-4">Frequently Asked Questions</h2>
        <div class="form-control mb-6">
          <input
            class="input input-bordered w-full"
            placeholder="Search FAQs..."
            [(ngModel)]="faqSearch"
            aria-label="Search FAQs"
          />
        </div>
        <div class="space-y-2">
          @for (faq of filteredFaqs(); track faq.question) {
            <div class="collapse collapse-arrow bg-base-100 rounded-lg">
              <input type="checkbox" [checked]="faq.open" (change)="faq.open = !faq.open" />
              <div class="collapse-title font-medium">{{ faq.question }}</div>
              <div class="collapse-content">
                <p class="text-base-content/70">{{ faq.answer }}</p>
              </div>
            </div>
          }
        </div>
      </div>
    </section>

    <!-- Final CTA -->
    <section class="py-20 bg-gradient-to-r from-primary to-secondary text-primary-content text-center">
      <div class="container mx-auto px-4">
        <h2 class="text-3xl font-bold mb-4">Ready to Start Winning?</h2>
        <p class="text-lg opacity-90 mb-8">Join AndyTipster today and get expert racing tips delivered daily.</p>
        <a routerLink="/auth/register" class="btn btn-warning btn-lg text-warning-content shadow-lg hover:scale-105 transition-transform">
          Get Started Free
        </a>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; }
    .animate-fade-in { animation: fadeIn 0.8s ease-out; }
    .animate-slide-up { animation: slideUp 0.6s ease-out; }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
    @keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
  `]
})
export class LandingComponent implements OnInit {
  private readonly plansService = inject(PlansService);

  plans = signal<Plan[]>([]);
  scrollY = signal(0);
  testimonialPage = signal(0);
  faqSearch = '';
  today = new Date();

  testimonials: Testimonial[] = [
    { name: 'James P.', quote: 'Best racing tips I\'ve ever followed. Consistent profits month after month.', rating: 5 },
    { name: 'Sarah M.', quote: 'Andy\'s analysis is spot on. The tips are well researched and profitable.', rating: 5 },
    { name: 'David R.', quote: 'Joined 3 months ago and already up over £1,500. Highly recommended.', rating: 5 },
    { name: 'Mike T.', quote: 'The daily tips have transformed my betting. Professional service.', rating: 4 },
    { name: 'Lisa K.', quote: 'Clear, concise tips with great reasoning. Worth every penny.', rating: 5 },
    { name: 'Tom W.', quote: 'Fantastic strike rate. Andy really knows his stuff.', rating: 5 },
    { name: 'Emma B.', quote: 'Signed up on a friend\'s recommendation, haven\'t looked back since.', rating: 5 },
    { name: 'Chris H.', quote: 'The P&L tracking is great - full transparency on results.', rating: 4 },
    { name: 'Rachel S.', quote: 'Best investment I\'ve made. The tips consistently deliver value.', rating: 5 },
  ];

  faqs: FaqItem[] = [
    { question: 'How are tips delivered?', answer: 'Tips are delivered via the web platform, email, and Telegram notifications as soon as they\'re published, typically before 10am each racing day.', open: false },
    { question: 'What is your strike rate?', answer: 'Our long-term verified strike rate is 87% across all selections. Full transparency with publicly viewable P&L records.', open: false },
    { question: 'Can I cancel anytime?', answer: 'Yes, you can cancel your subscription at any time. You\'ll retain access until the end of your current billing period.', open: false },
    { question: 'What sports do you cover?', answer: 'We specialise in UK and Irish horse racing, covering all major meetings and festivals throughout the year.', open: false },
    { question: 'Is there a free trial?', answer: 'Yes, selected plans include a free trial period so you can experience our service before committing.', open: false },
    { question: 'How do I contact support?', answer: 'You can reach us via the in-app chat, email, or through our Telegram group. We typically respond within a few hours.', open: false },
    { question: 'What payment methods do you accept?', answer: 'We accept PayPal and all major credit/debit cards via Stripe. All payments are processed securely.', open: false },
    { question: 'Do you offer refunds?', answer: 'We offer a money-back guarantee if you\'re not satisfied within the first 7 days of your subscription.', open: false },
  ];

  visibleTestimonials = signal<Testimonial[]>([]);

  ngOnInit(): void {
    this.plansService.getPlans().subscribe(plans => {
      this.plans.set(plans.filter(p => p.isActive).slice(0, 3));
    });
    this.updateTestimonials();
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrollY.set(window.scrollY);
  }

  filteredFaqs(): FaqItem[] {
    if (!this.faqSearch) return this.faqs;
    const q = this.faqSearch.toLowerCase();
    return this.faqs.filter(f =>
      f.question.toLowerCase().includes(q) || f.answer.toLowerCase().includes(q)
    );
  }

  private updateTestimonials(): void {
    const page = this.testimonialPage();
    this.visibleTestimonials.set(this.testimonials.slice(page * 3, page * 3 + 3));

    // Auto-rotate testimonials
    setInterval(() => {
      const next = (this.testimonialPage() + 1) % 3;
      this.testimonialPage.set(next);
      this.visibleTestimonials.set(this.testimonials.slice(next * 3, next * 3 + 3));
    }, 5000);
  }
}
