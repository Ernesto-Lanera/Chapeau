let TOTAL = 0;
let ORDER_ID = 0;

const payments = [];

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

function showCheckout() {
    const orderSection = document.getElementById('orderSection');
    const checkoutSection = document.getElementById('checkoutSection');

    if (orderSection) orderSection.style.display = 'none';
    if (checkoutSection) checkoutSection.style.display = 'block';

    updateRemaining();
}

function hideCheckout() {
    const orderSection = document.getElementById('orderSection');
    const checkoutSection = document.getElementById('checkoutSection');

    if (orderSection) orderSection.style.display = 'block';
    if (checkoutSection) checkoutSection.style.display = 'none';
}

function calcStatus() {
    updateRemaining();
}

function updateRemaining() {
    const amountInput = document.getElementById('amountInput');
    const el = document.getElementById('remainingDisplay');
    const container = document.getElementById('remainingContainer');
    const tipRow = document.getElementById('tipRow');
    const tipDisp = document.getElementById('tipDisp');

    if (!amountInput || !el || !container) return;

    const val = parseFloat(amountInput.value) || 0;
    const rem = TOTAL - val;
    const tip = val > TOTAL ? val - TOTAL : 0;

    el.textContent = rem <= 0 ? '€0,00' : fmt(rem);

    if (tipRow && tipDisp) {
        tipRow.style.display = tip > 0 ? 'flex' : 'none';
        tipDisp.textContent = fmt(tip);
    }

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

function addPayment() {
    const amountInput = document.getElementById('amountInput');
    const paymentMethod = document.getElementById('paymentMethod');
    if (!amountInput || !paymentMethod) return;

    const amount = parseFloat(amountInput.value);
    if (!amount || amount <= 0) {
        alert('Voer een geldig bedrag in.');
        return;
    }

    payments.push({ amount, method: paymentMethod.value });
    amountInput.value = '';
    updateRemaining();
    renderPayments();
}

function renderPayments() {
    const list = document.getElementById('paymentsList');
    const container = document.getElementById('paymentsContainer');
    const stillToPay = document.getElementById('stillToPay');
    if (!list || !container || !stillToPay) return;

    container.innerHTML = '';

    const totalPaid = payments.reduce((sum, p) => sum + p.amount, 0);
    const remaining = Math.max(0, TOTAL - totalPaid);

    payments.forEach((p, i) => {
        const row = document.createElement('div');
        row.className = 'd-flex justify-content-between small mb-1';
        row.textContent = `${i + 1}. ${p.method} - ${fmt(p.amount)}`;
        container.appendChild(row);
    });

    stillToPay.textContent = remaining <= 0 ? '€0,00' : fmt(remaining);
    list.style.display = 'block';
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