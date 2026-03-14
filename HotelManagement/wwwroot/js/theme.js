/**
 * theme.js - Premium Scroll & Theme Orchestration
 * Minh Quang Luxury Hotel Management System
 */

document.addEventListener('DOMContentLoaded', () => {
    // 1. INITIALIZE LENIS SMOOTH SCROLL
    const lenis = new Lenis({
        duration: 1.2,
        easing: (t) => Math.min(1, 1.001 - Math.pow(2, -10 * t)),
        direction: 'vertical',
        gestureDirection: 'vertical',
        smooth: true,
        mouseMultiplier: 1,
        smoothTouch: false,
        touchMultiplier: 2,
        infinite: false,
    });

    function raf(time) {
        lenis.raf(time);
        requestAnimationFrame(raf);
    }
    requestAnimationFrame(raf);

    // Integrate with ScrollTrigger if needed later
    window.lenis = lenis;

    // 2. SCROLL REVEAL ANIMATIONS
    const revealOptions = {
        threshold: 0.15,
        rootMargin: '0px 0px -50px 0px'
    };

    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('reveal-active');
                // Optional: stop observing after reveal
                // revealObserver.unobserve(entry.target);
            }
        });
    }, revealOptions);

    // Elements to reveal
    const revealElements = document.querySelectorAll('.reveal, .reveal-up, .reveal-left, .reveal-right, .reveal-scale');
    revealElements.forEach(el => revealObserver.observe(el));

    // 3. PARALLAX EFFECT FOR HERO IMAGE
    const heroImage = document.querySelector('.hero-parallax');
    if (heroImage) {
        lenis.on('scroll', ({ scroll }) => {
            heroImage.style.transform = `translateY(${scroll * 0.3}px)`;
        });
    }

    // 4. NAV SCROLL EFFECT (Alternative if public-layout.js is simple)
    const nav = document.querySelector('.glass-nav-public');
    if (nav) {
        lenis.on('scroll', ({ scroll }) => {
            if (scroll > 50) {
                nav.classList.add('is-scrolled');
            } else {
                nav.classList.remove('is-scrolled');
            }
        });
    }
});
