import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  OnDestroy,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  color: string;
  size: number;
  rotation: number;
  rotationSpeed: number;
  opacity: number;
}

/**
 * Confetti animation component.
 * Triggered on subscription purchase or other celebratory events.
 */
@Component({
  selector: 'app-confetti',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (active()) {
      <canvas #canvas class="fixed inset-0 pointer-events-none z-[9999]" aria-hidden="true"></canvas>
    }
  `,
})
export class ConfettiComponent implements OnInit, OnDestroy {
  /** Whether to trigger the confetti animation */
  readonly trigger = input(false);

  readonly active = signal(false);
  private canvas = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private animationFrame: number | null = null;
  private particles: Particle[] = [];

  private readonly colors = [
    '#ff6b6b', '#ffd93d', '#6bcb77', '#4d96ff',
    '#ff9a3c', '#a66cff', '#ff6b9d', '#00d2d3',
  ];

  ngOnInit(): void {
    if (this.trigger()) {
      this.fire();
    }
  }

  ngOnDestroy(): void {
    this.stop();
  }

  fire(): void {
    this.active.set(true);
    // Wait for canvas to render
    requestAnimationFrame(() => {
      this.initCanvas();
      this.createParticles();
      this.animate();
      // Auto-stop after 3 seconds
      setTimeout(() => this.stop(), 3000);
    });
  }

  private initCanvas(): void {
    const canvasEl = this.canvas();
    if (!canvasEl) return;
    const el = canvasEl.nativeElement;
    el.width = window.innerWidth;
    el.height = window.innerHeight;
  }

  private createParticles(): void {
    this.particles = [];
    const count = 150;
    for (let i = 0; i < count; i++) {
      this.particles.push({
        x: window.innerWidth / 2 + (Math.random() - 0.5) * 200,
        y: window.innerHeight / 2 - Math.random() * 200,
        vx: (Math.random() - 0.5) * 15,
        vy: Math.random() * -15 - 5,
        color: this.colors[Math.floor(Math.random() * this.colors.length)],
        size: Math.random() * 8 + 4,
        rotation: Math.random() * 360,
        rotationSpeed: (Math.random() - 0.5) * 10,
        opacity: 1,
      });
    }
  }

  private animate(): void {
    const canvasEl = this.canvas();
    if (!canvasEl) return;
    const ctx = canvasEl.nativeElement.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, canvasEl.nativeElement.width, canvasEl.nativeElement.height);

    let activeParticles = 0;

    for (const p of this.particles) {
      if (p.opacity <= 0) continue;
      activeParticles++;

      p.vy += 0.5; // gravity
      p.x += p.vx;
      p.y += p.vy;
      p.rotation += p.rotationSpeed;
      p.opacity -= 0.008;

      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.rotate((p.rotation * Math.PI) / 180);
      ctx.globalAlpha = Math.max(0, p.opacity);
      ctx.fillStyle = p.color;
      ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size / 2);
      ctx.restore();
    }

    if (activeParticles > 0) {
      this.animationFrame = requestAnimationFrame(() => this.animate());
    } else {
      this.stop();
    }
  }

  private stop(): void {
    if (this.animationFrame !== null) {
      cancelAnimationFrame(this.animationFrame);
      this.animationFrame = null;
    }
    this.active.set(false);
    this.particles = [];
  }
}
