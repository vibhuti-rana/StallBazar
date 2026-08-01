const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

document.querySelectorAll("[data-carousel]").forEach((carousel) => {
    const slides = carousel.querySelectorAll(".hero-slide, .photo-slide");
    if (slides.length < 2) {
        return;
    }

    let index = 0;
    let timer;
    const controls = document.createElement("div");
    controls.className = "carousel-dots";
    controls.setAttribute("aria-label", "Choose image");
    carousel.setAttribute("aria-roledescription", "carousel");

    const showSlide = (nextIndex) => {
        slides[index].classList.remove("active");
        slides[index].setAttribute("aria-hidden", "true");
        controls.children[index]?.removeAttribute("aria-current");
        index = (nextIndex + slides.length) % slides.length;
        slides[index].classList.add("active");
        slides[index].setAttribute("aria-hidden", "false");
        controls.children[index]?.setAttribute("aria-current", "true");
    };

    slides.forEach((slide, slideIndex) => {
        slide.setAttribute("aria-hidden", slideIndex === 0 ? "false" : "true");
        const button = document.createElement("button");
        button.type = "button";
        button.setAttribute("aria-label", `Show image ${slideIndex + 1} of ${slides.length}`);
        if (slideIndex === 0) {
            button.setAttribute("aria-current", "true");
        }
        button.addEventListener("click", () => showSlide(slideIndex));
        controls.appendChild(button);
    });
    carousel.appendChild(controls);

    const start = () => {
        if (!prefersReducedMotion) {
            timer = window.setInterval(() => showSlide(index + 1), 6000);
        }
    };
    const stop = () => window.clearInterval(timer);
    carousel.addEventListener("mouseenter", stop);
    carousel.addEventListener("mouseleave", start);
    carousel.addEventListener("focusin", stop);
    carousel.addEventListener("focusout", start);
    start();
});

const savedTheme = window.localStorage.getItem("stallbazar-theme");
if (savedTheme) {
    document.documentElement.dataset.theme = savedTheme;
}

document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
        const nextTheme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
        document.documentElement.dataset.theme = nextTheme;
        window.localStorage.setItem("stallbazar-theme", nextTheme);
    });
});

document.querySelectorAll("[data-password-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
        const field = button.closest(".password-field");
        const input = field?.querySelector("[data-password-input]");
        if (!input) {
            return;
        }

        const isHidden = input.getAttribute("type") === "password";
        input.setAttribute("type", isHidden ? "text" : "password");
        button.textContent = isHidden ? "Hide" : "Show";
    });
});

document.querySelectorAll(".auto-dismiss").forEach((alert) => {
    window.setTimeout(() => {
        alert.classList.add("alert-hiding");
        window.setTimeout(() => {
            const instance = bootstrap.Alert.getOrCreateInstance(alert);
            instance.close();
        }, 260);
    }, 4000);
});

const siteHeader = document.querySelector("[data-site-header]");
const progressBar = document.querySelector("[data-scroll-progress]");
let scrollTicking = false;

const updateScrollUi = () => {
    const scrollTop = window.scrollY || document.documentElement.scrollTop;
    const scrollable = document.documentElement.scrollHeight - window.innerHeight;
    const progress = scrollable > 0 ? Math.min(1, scrollTop / scrollable) : 0;

    siteHeader?.classList.toggle("is-scrolled", scrollTop > 18);
    if (progressBar) {
        progressBar.style.transform = `scaleX(${progress})`;
    }
    scrollTicking = false;
};

window.addEventListener("scroll", () => {
    if (!scrollTicking) {
        window.requestAnimationFrame(updateScrollUi);
        scrollTicking = true;
    }
}, { passive: true });
updateScrollUi();

document.querySelectorAll(".site-header a[href]").forEach((link) => {
    const url = new URL(link.href, window.location.origin);
    const currentPath = window.location.pathname.replace(/\/$/, "") || "/";
    const linkPath = url.pathname.replace(/\/$/, "") || "/";
    if (url.origin === window.location.origin && linkPath === currentPath) {
        link.setAttribute("aria-current", "page");
    }
});

document.querySelectorAll("#mainNav a").forEach((link) => {
    link.addEventListener("click", () => {
        const nav = document.getElementById("mainNav");
        if (nav?.classList.contains("show") && window.bootstrap) {
            bootstrap.Collapse.getOrCreateInstance(nav).hide();
        }
    });
});

document.querySelectorAll("form[data-confirm]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        if (!window.confirm(form.dataset.confirm)) {
            event.preventDefault();
        }
    });
});

document.querySelectorAll("[data-event-builder]").forEach((builder) => {
    const form = builder.querySelector(".event-builder-form");
    if (!form) {
        return;
    }

    const setText = (selector, value, fallback) => {
        const target = builder.querySelector(selector);
        if (target) {
            target.textContent = value?.trim() || fallback;
        }
    };

    const formatNumber = (value) => {
        const number = Number(value);
        return Number.isFinite(number) && number > 0 ? new Intl.NumberFormat("en-NP").format(number) : "Not stated";
    };

    const updatePreview = () => {
        const name = form.querySelector("[data-preview-input='name']")?.value || "";
        const venue = form.querySelector("[data-preview-input='venue']")?.value || "";
        const category = form.querySelector("[data-preview-input='category']")?.value || "Event";
        const description = form.querySelector("[data-preview-input='description']")?.value || "";
        const price = form.querySelector("[data-preview-input='price']")?.value || "";
        const footfall = form.querySelector("[data-preview-input='footfall']")?.value || "";
        const dateValue = form.querySelector("[data-preview-input='date']")?.value;
        const date = dateValue ? new Date(dateValue) : null;

        setText("[data-preview-name]", name, "Your event name");
        setText("[data-preview-venue]", venue, "Venue and city");
        setText("[data-preview-category]", category, "Event");
        setText("[data-preview-description]", description, "Your event description will appear here for vendors to review before they open the full listing.");
        setText("[data-preview-price]", price ? formatNumber(price) : "0", "0");
        setText("[data-preview-footfall]", formatNumber(footfall), "Not stated");
        if (date && !Number.isNaN(date.getTime())) {
            setText("[data-preview-month]", date.toLocaleDateString("en", { month: "short" }), "---");
            setText("[data-preview-day]", String(date.getDate()).padStart(2, "0"), "--");
        }
    };

    const readinessRules = {
        Name: () => ["Name", "Venue", "Category"].every((name) => form.elements[name]?.value?.trim()),
        StartsAt: () => ["StartsAt", "EndsAt", "ApplicationDeadline"].every((name) => form.elements[name]?.value),
        Description: () => (form.elements.Description?.value?.trim().length || 0) >= 80,
        VendorRequirements: () => ["Facilities", "VendorRequirements", "CancellationPolicy"].every((name) => form.elements[name]?.value?.trim()),
        EventImage: () => Boolean(form.elements.EventImage?.files?.length) || builder.dataset.existingCover === "true",
        MapImage: () => Boolean(form.elements.MapImage?.files?.length) || builder.dataset.existingMap === "true"
    };

    const updateReadiness = () => {
        Object.entries(readinessRules).forEach(([name, rule]) => {
            builder.querySelector(`[data-readiness='${name}']`)?.classList.toggle("complete", Boolean(rule()));
        });
    };

    form.querySelectorAll("input, select, textarea").forEach((control) => {
        control.addEventListener("input", () => {
            updatePreview();
            updateReadiness();
        });
        control.addEventListener("change", updateReadiness);
    });

    form.querySelectorAll("textarea[maxlength]").forEach((textarea) => {
        const counter = builder.querySelector(`[data-character-count='${textarea.name}']`);
        const updateCount = () => {
            if (counter) {
                counter.textContent = String(textarea.value.length);
            }
        };
        textarea.addEventListener("input", updateCount);
        updateCount();
    });

    const coverUpload = form.querySelector("[data-cover-upload]");
    coverUpload?.addEventListener("change", () => {
        const file = coverUpload.files?.[0];
        const cover = builder.querySelector("[data-preview-cover]");
        if (!file || !cover) {
            return;
        }
        const reader = new FileReader();
        reader.addEventListener("load", () => {
            cover.style.backgroundImage = `url('${reader.result}')`;
        });
        reader.readAsDataURL(file);
    });

    updatePreview();
    updateReadiness();
});

document.querySelectorAll("[data-stall-builder]").forEach((builder) => {
    const categoryInputs = builder.querySelectorAll("[data-stall-category]");
    const sizeOutput = builder.querySelector("[data-size-output]");
    const lengthOutput = builder.querySelector("[data-length-output]");
    const breadthOutput = builder.querySelector("[data-breadth-output]");
    const derivedSize = builder.querySelector("[data-derived-size]");
    const derivedDescription = builder.querySelector("[data-derived-description]");
    const previewTier = builder.querySelector("[data-preview-tier]");
    const previewSize = builder.querySelector("[data-preview-size]");
    const previewNumber = builder.querySelector("[data-preview-number]");
    const previewQuantity = builder.querySelector("[data-preview-quantity]");
    const priceInput = builder.querySelector("[data-price-input]");
    const quantityInput = builder.querySelector("[name='Quantity']");
    const prefixInput = builder.querySelector("[name='NumberPrefix']");
    const startInput = builder.querySelector("[name='StartingNumber']");

    const updateNumberPreview = () => {
        if (!previewNumber) {
            return;
        }

        const prefix = prefixInput?.value?.trim().toUpperCase() || "";
        const start = Number(startInput?.value || 1);
        const padded = Number.isFinite(start) ? String(start).padStart(2, "0") : "01";
        previewNumber.textContent = prefix ? `${prefix}${padded}` : padded;
        if (previewQuantity && quantityInput) {
            previewQuantity.textContent = quantityInput.value || "1";
        }
    };

    const syncCategory = (allowPriceSuggestion) => {
        const selected = Array.from(categoryInputs).find((input) => input.checked);
        if (!selected) {
            return;
        }

        categoryInputs.forEach((input) => {
            input.closest(".stall-category-card")?.classList.toggle("selected", input === selected);
        });

        const size = selected.dataset.size || "3m x 3m";
        const description = selected.dataset.description || "";
        if (sizeOutput) {
            sizeOutput.value = size;
        }
        if (lengthOutput) {
            lengthOutput.value = selected.dataset.length || "3";
        }
        if (breadthOutput) {
            breadthOutput.value = selected.dataset.breadth || "3";
        }
        if (derivedSize) {
            derivedSize.textContent = size;
        }
        if (derivedDescription) {
            derivedDescription.textContent = description;
        }
        if (previewTier) {
            previewTier.textContent = selected.value;
        }
        if (previewSize) {
            previewSize.textContent = size;
        }
        if (priceInput && allowPriceSuggestion && (!priceInput.value || priceInput.dataset.autofilled === "true")) {
            priceInput.value = selected.dataset.price || priceInput.value;
            priceInput.dataset.autofilled = "true";
        }
    };

    categoryInputs.forEach((input) => {
        input.addEventListener("change", () => syncCategory(true));
    });
    [quantityInput, prefixInput, startInput].forEach((input) => {
        input?.addEventListener("input", updateNumberPreview);
    });
    priceInput?.addEventListener("input", () => {
        priceInput.dataset.autofilled = "false";
    });

    syncCategory(false);
    updateNumberPreview();
});

if (!prefersReducedMotion && window.matchMedia("(hover: hover)").matches) {
    document.querySelectorAll(".event-card, .metric, .category-card-link, .platform-card, .story-card, .panel").forEach((card) => {
        card.addEventListener("pointermove", (event) => {
            const bounds = card.getBoundingClientRect();
            card.style.setProperty("--pointer-x", `${event.clientX - bounds.left}px`);
            card.style.setProperty("--pointer-y", `${event.clientY - bounds.top}px`);
        });
    });
}

const revealTargets = document.querySelectorAll(
    ".swap-hero-inner, .hero-device, .brand-strip, .homepage-overview, .overview-grid article, .swap-section, .kpi-band, .quote-rail article, .role-showcase, .role-showcase-grid article, .platform-card, .story-card, .process-band, .process-band li, .article-grid article, .faq-section, .faq-grid article, .swap-demo, .page-head, .panel, .event-card, .layout-panel, .auth-photo-panel, .auth-card, .image-swipe-banner, .browse-hero-copy, .info-hero > div, .metric, .notice-card, .category-card-link, .list-card, .stall-tile"
);

if (!prefersReducedMotion && "IntersectionObserver" in window) {
    document.documentElement.classList.add("reveal-ready");
    const revealGroups = document.querySelectorAll(
        ".overview-grid, .role-showcase-grid, .quote-rail, .platform-grid, .story-grid, .process-band ol, .article-grid, .faq-grid, .metric-grid, .notice-row, .category-showcase, .event-grid, .list-stack, .stall-grid"
    );
    revealGroups.forEach((group) => {
        Array.from(group.children).forEach((child, index) => {
            child.style.setProperty("--reveal-delay", `${Math.min(index, 6) * 70}ms`);
        });
    });

    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                entry.target.classList.add("is-visible");
                revealObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.08, rootMargin: "0px 0px -48px" });

    revealTargets.forEach((target) => revealObserver.observe(target));
} else {
    revealTargets.forEach((target) => target.classList.add("is-visible"));
}
