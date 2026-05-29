let TOTAL = 0;
let ORDER_ID = 0;
let TABLE_NUMBER = 0;
let payments = [];

document.addEventListener('DOMContentLoaded', function () {
    const dataEl = document.getElementById('paymentData');
    if (dataEl) {
        TOTAL = parseFloat(dataEl.getAttribute('data-total')) || 0;
        ORDER_ID = parseInt(dataEl.getAttribute('data-order-id')) || 0;
        TABLE_NUMBER = parseInt(dataEl.getAttribute('data-table-number')) || 0;
    }
});

// ── Helpers ──────────────────────────────────────────────

function fmt(n) {
    return '€' + Math.abs(n).toFixed(2).replace('.', ',');
}

function paidSoFar() {
    return payments.reduce((s, p) => s + p.amount, 0);
}

// ── Section navigation ───────────────────────────────────

function showCheckout() {
    document.getElementById('orderSection').style.display = 'none';
    document.getElementById('checkoutSection').style.display = 'block';
}

function hideCheckout() {
    document.getElementById('checkoutSection').style.display = 'none';
    document.getElementById('orderSection').style.display = 'block';
}

// ── Checkout logic ───────────────────────────────────────

function calcStatus() {
    const val = parseFloat(document.getElementById('amountInput').value) || 0;
    const remaining = TOTAL - paidSoFar();
    const diff = val - remaining;

    const tipRow = document.getElementById('tipRow');
    const remainContainer = document.getElementById('remainingContainer');
    const remainDisp = document.getElementById('remainingDisplay');

    if (diff > 0.001) {
        // Overpay: show tip badge
        tipRow.style.display = 'flex';
        document.getElementById('tipDisp').textContent = fmt(diff);
        remainDisp.textContent = '€0,00';
        remainDisp.style.color = '#059669';
        remainContainer.classList.remove('overpay');
    } else {
        tipRow.style.display = 'none';
        const left = remaining - val;
        remainDisp.textContent = fmt(left);
        remainDisp.style.color = left < 0 ? '#dc2626' : '#059669';
        remainContainer.classList.toggle('overpay', left < 0);
    }
}

function toggleSplit() {
    const on = document.getElementById('splitCheck').checked;
    document.getElementById('splitSection').style.display = on ? 'block' : 'none';
    if (on) {
        applySplit();
    } else {
        document.getElementById('amountInput').value = '';
        calcStatus();
    }
}

function applySplit() {
    const n = parseInt(document.getElementById('splitCount').value) || 1;
    const remaining = TOTAL - paidSoFar();
    const pp = remaining / n;
    document.getElementById('perPerson').textContent = fmt(pp);
    document.getElementById('amountInput').value = pp.toFixed(2);
    calcStatus();
}

function addPayment() {
    const amount = parseFloat(document.getElementById('amountInput').value);
    if (!amount || amount <= 0) {
        alert('Voer een geldig bedrag in.');
        return;
    }

    const method = document.getElementById('paymentMethod').value;
    const remaining = TOTAL - paidSoFar();
    const tip = parseFloat(Math.max(0, amount - remaining).toFixed(2));

    payments.push({ amount: parseFloat(amount.toFixed(2)), method, tip });
    renderPayments();

    // Reset inputs
    document.getElementById('amountInput').value = '';
    document.getElementById('splitCheck').checked = false;
    document.getElementById('splitSection').style.display = 'none';
    document.getElementById('tipRow').style.display = 'none';
    calcStatus();
}

function renderPayments() {
    const container = document.getElementById('paymentsContainer');
    const list = document.getElementById('paymentsList');
    list.style.display = payments.length > 0 ? 'block' : 'none';

    container.innerHTML = payments.map(p =>
        `<div class="payment-added-row">
            <span>${p.method}${p.tip > 0.001
            ? ' <span class="badge bg-success ms-1">tip ' + fmt(p.tip) + '</span>'
            : ''}</span>
            <strong>${fmt(p.amount)}</strong>
        </div>`
    ).join('');

    const stillLeft = Math.max(0, TOTAL - paidSoFar());
    document.getElementById('stillToPay').textContent = fmt(stillLeft);
}

function doPay() {
    if (payments.length === 0) {
        alert('Voeg eerst een betaling toe.');
        return;
    }

    const totalPaid = paidSoFar();
    const totalTip = payments.reduce((s, p) => s + (p.tip || 0), 0);
    const feedback = document.getElementById('feedbackInput').value || '';

    // Confirmation subtitle
    let sub = fmt(totalPaid);
    if (totalTip > 0.001) sub += ' (tip: ' + fmt(totalTip) + ')';
    document.getElementById('confirmSub').textContent = sub;

    // Confirmation details
    let details =
        `<div class="confirm-detail-row">
            <span>Table</span><strong>${TABLE_NUMBER}</strong>
        </div>
        <div class="confirm-detail-row">
            <span>Order</span><strong>#${ORDER_ID}</strong>
        </div><hr/>`;

    payments.forEach(p => {
        details +=
            `<div class="confirm-detail-row">
                <span>${p.method}</span>
                <strong>${fmt(p.amount)}</strong>
            </div>`;
    });

    if (totalTip > 0.001) {
        details +=
            `<hr/>
            <div class="confirm-detail-row">
                <span>Tip</span>
                <strong class="text-success">${fmt(totalTip)}</strong>
            </div>`;
    }

    if (feedback) {
        details +=
            `<hr/>
            <div style="font-size: 12px; font-style: italic; color: #6b7280;">
                <strong>Feedback:</strong> "${feedback}"
            </div>`;
    }

    document.getElementById('confirmDetails').innerHTML = details;
    document.getElementById('checkoutSection').style.display = 'none';
    document.getElementById('confirmSection').style.display = 'block';

    // Save to database
    savePaymentToDatabase(totalPaid, totalTip, feedback);
}

function savePaymentToDatabase(totalPaid, totalTip, feedback) {
    const paymentMethod = payments.map(p => p.method).join(', ');

    // POST to server to save payment
    fetch('/Payment/SavePayment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            orderId: ORDER_ID,
            tableNumber: TABLE_NUMBER,
            totalPaidAmount: totalPaid,
            tipAmount: totalTip,
            paymentMethod: paymentMethod,
            feedback: feedback
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Payment saved successfully');
            } else {
                console.error('Failed to save payment:', data.message);
            }
        })
        .catch(error => console.error('Error:', error));
}