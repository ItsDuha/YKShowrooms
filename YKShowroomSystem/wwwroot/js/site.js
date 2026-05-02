// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Light/Dark mode toggle and language/RTL/translation logic
(function () {

    // --- Translation Dictionary ---
    var arTranslations = {
        'Showrooms Service': 'خدمة صالات العرض',
        'Welcome to Showrooms Service': 'مرحبًا بكم في خدمة صالات العرض',
        'Check In': 'تسجيل الوصول',
        'Full Name': 'الاسم الكامل',
        'Phone Number': 'رقم الهاتف',
        'Email (Optional)': 'البريد الإلكتروني (اختياري)',
        'CPR Number': 'الرقم الشخصي',
        'Showroom': 'صالة العرض',
        'Select a showroom': 'يرجى اختيار صالة عرض',
        'Car Model of Interest': 'طراز السيارة المفضل',
        'Not sure yet': 'غير متأكد بعد',
        'Assigned Salesperson': 'مندوب المبيعات المعين',
        'Select a salesperson (optional)': 'اختر مندوب مبيعات (اختياري)',
        'Notes (Optional)': 'ملاحظات (اختياري)',
        'Complete Check-in': 'إكمال التسجيل',
        'Rate your experience': 'قيّم تجربتك',
        'We value your feedback': 'نقدّر ملاحظاتك',
        'Tell us about your experience...': 'أخبرنا عن تجربتك...',
        'Submit Feedback': 'إرسال الملاحظات',
        'Thank you for your feedback!': 'شكرًا لملاحظاتك!',
        'We appreciate your time': 'نقدّر وقتك',
        'Name is required': 'الاسم مطلوب',
        'Phone must be exactly 8 digits': 'يجب أن يكون رقم الهاتف 8 أرقام',
        'CPR must be exactly 9 digits': 'يجب أن يكون الرقم الشخصي 9 أرقام',
        'Please select a showroom': 'يرجى اختيار صالة عرض',
    };

    // --- Translate all text nodes on page ---
    function translatePage(lang) {
        // Apply RTL / LTR
        if (lang === 'ar') {
            document.documentElement.setAttribute('dir', 'rtl');
            document.documentElement.setAttribute('lang', 'ar');
            document.body.setAttribute('dir', 'rtl');
            document.body.classList.add('rtl');
        } else {
            document.documentElement.setAttribute('dir', 'ltr');
            document.documentElement.setAttribute('lang', 'en');
            document.body.setAttribute('dir', 'ltr');
            document.body.classList.remove('rtl');
        }

        // Walk all text nodes and translate
        var walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            null,
            false
        );

        var node;
        while (node = walker.nextNode()) {
            var text = node.textContent.trim();
            if (!text) continue;

            if (lang === 'ar' && arTranslations[text]) {
                node.textContent = arTranslations[text];
            } else if (lang === 'en') {
                var arKey = Object.keys(arTranslations).find(function (key) {
                    return arTranslations[key] === text;
                });
                if (arKey) node.textContent = arKey;
            }
        }

        // Translate placeholders
        document.querySelectorAll('[placeholder]').forEach(function (el) {
            var text = el.placeholder.trim();
            if (lang === 'ar' && arTranslations[text]) {
                el.placeholder = arTranslations[text];
            } else if (lang === 'en') {
                var arKey = Object.keys(arTranslations).find(function (key) {
                    return arTranslations[key] === text;
                });
                if (arKey) el.placeholder = arKey;
            }
        });

        // Save to localStorage
        localStorage.setItem('lang', lang);
        localStorage.setItem('siteLang', lang);

        // Update language button label if present
        var langBtnLabel = document.getElementById('langBtnLabel');
        if (langBtnLabel) {
            langBtnLabel.innerText = lang === 'ar' ? 'العربية' : 'English';
        }
    }

    // --- Expose globally so layout button and page scripts can call it ---
    window.translatePage = translatePage;

    // --- Toggle function called by the navbar button ---
    window.toggleLanguage = function () {
        var current = localStorage.getItem('lang') || 'en';
        var next = current === 'en' ? 'ar' : 'en';
        translatePage(next);
    };

    // --- Theme Setter ---
    function setTheme(dark) {
        if (dark) {
            document.body.classList.add('dark-mode');
            localStorage.setItem('theme', 'dark');
        } else {
            document.body.classList.remove('dark-mode');
            localStorage.setItem('theme', 'light');
        }
        var checkbox = document.getElementById('theme-checkbox');
        if (checkbox) checkbox.checked = dark;
    }

    function initThemeToggle() {
        var checkbox = document.getElementById('theme-checkbox');
        if (!checkbox) return;
        var savedTheme = localStorage.getItem('theme');
        setTheme(savedTheme === 'dark');
        checkbox.addEventListener('change', function () {
            setTheme(this.checked);
        });
    }

    // --- Init on DOMContentLoaded ---
    document.addEventListener('DOMContentLoaded', function () {
        initThemeToggle();

        // Apply stored language on every page
        var storedLang = localStorage.getItem('lang') || 'en';
        translatePage(storedLang);
    });

    // Apply RTL immediately (before DOM loads) for fast render
    (function () {
        var storedLang = localStorage.getItem('lang');
        if (storedLang === 'ar') {
            document.documentElement.setAttribute('dir', 'rtl');
            document.documentElement.setAttribute('lang', 'ar');
        }
    })();

})();
