
let TOTAL = 0;
let ORDER_ID = 0;

document.addEventListener('DOMContentLoaded', function() {

    const totalEl = document.querySelector('[data-total]');
    if (totalEl) {
        TOTAL = parseFloat(totalEl.getAttribute('data-total')) || 0;
        ORDER_ID = parseInt(totalEl.getAttribute('data-order-id')) || 0;
    }

    const amountInput = document.getElementById('amountInput');
    if (amountInput) {
        amountInput.addEventListener('input', updateRemaining);
    }

    const splitCheck = document.getElementById('splitCheck');
    if (splitCheck) {
        splitCheck.addEventListener('change', toggleSplit);
    }

    const splitCount = document.getElementById('splitCount');
    if (splitCount) {
        splitCount.addEventListener('change', applySplit);
    }

    const payButton = document.querySelector('[data-pay-btn]');
    if (payButton) {
        payButton.addEventListener('click', doPay);
    }
});

function fmt(n) {
    return '€' + Math.abs(n).toFixed(2);
}

function updateRemaining() {
    const val = parseFloat(document.getElementById('amountInput').value) || 0;
    const rem = TOTAL - val;
    const el = document.getElementById('remainingDisplay');
    const container = document.getElementById('remainingContainer');
    
    el.textContent = rem < 0 ? '-' + fmt(rem) + ' (te veel)' : fmt(rem);
    
    if (rem < 0) {
        el.style.color = '#dc2626';
        container.classList.add('overpay');
    } else {
        el.style.color = '#059669';
        container.classList.remove('overpay');
    }
}

function toggleSplit() {
    const on = document.getElementById('splitCheck').checked;
    const splitSection = document.getElementById('splitSection');
    
    if (splitSection) {
        splitSection.style.display = on ? 'block' : 'none';
    }
    
    if (on) {
        applySplit();
    } else {
        const amountInput = document.getElementById('amountInput');
        if (amountInput) {
            amountInput.value = '';
        }
        updateRemaining();
    }
}

function applySplit() {
    const splitCountSelect = document.getElementById('splitCount');
    if (!splitCountSelect) return;

    const n = parseInt(splitCountSelect.value) || 1;
    const pp = TOTAL / n;
    
    const perPersonEl = document.getElementById('perPerson');
    if (perPersonEl) {
        perPersonEl.textContent = fmt(pp);
    }

    const amountInput = document.getElementById('amountInput');
    if (amountInput) {
        amountInput.value = pp.toFixed(2);
    }
    
    updateRemaining();
}

function doPay() {
    const amountInput = document.getElementById('amountInput');
    if (!amountInput) return;

    const amount = parseFloat(amountInput.value);
    
    if (!amount || amount <= 0) {
        alert('Voer een geldig bedrag in.');
        return;
    }

    const paymentMethod = document.getElementById('paymentMethod');
    const method = paymentMethod ? paymentMethod.value : 'Card';
    
    if (!ORDER_ID) {
        alert('Bestelling ID ontbreekt.');
        return;
    }

    window.location.href = `/Payment/Confirmation?orderId=${ORDER_ID}&amount=${amount.toFixed(2)}&method=${method}`;
}