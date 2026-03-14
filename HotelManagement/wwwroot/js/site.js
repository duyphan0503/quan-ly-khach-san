/**
 * Hotel Management System - Core JavaScript
 * Implements intersection observers for animations and liquid glass effects.
 */

document.addEventListener('DOMContentLoaded', function () {
    // 1. Reveal on Scroll Observer
    const revealObserverOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('show');
                // Once shown, we can stop observing it
                revealObserver.unobserve(entry.target);
            }
        });
    }, revealObserverOptions);

    const revealElements = document.querySelectorAll('.reveal-up');
    revealElements.forEach(el => revealObserver.observe(el));

    // 2. Sticky Header Effect
    const header = document.querySelector('nav');
    if (header) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 50) {
                header.classList.add('bg-surface-1/90', 'backdrop-blur-xl', 'py-3', 'shadow-2xl');
                header.classList.remove('bg-transparent', 'py-6');
            } else {
                header.classList.add('bg-transparent', 'py-6');
                header.classList.remove('bg-surface-1/90', 'backdrop-blur-xl', 'py-3', 'shadow-2xl');
            }
        });
    }

    // 3. Smooth Scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#') {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });

    // 4. Parallax Effect for Hero (Subtle)
    const heroBg = document.querySelector('.hero-parallax');
    if (heroBg) {
        window.addEventListener('scroll', () => {
            const scroll = window.scrollY;
            heroBg.style.transform = `scale(${1 + scroll * 0.0005}) translateY(${scroll * 0.1}px)`;
        });
    }
});
