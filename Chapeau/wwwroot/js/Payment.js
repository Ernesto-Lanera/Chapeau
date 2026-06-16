let TOTAL = 0;
let ORDER_ID = 0;
let ORDER_IDS = [];
let TABLE_NUMBER = 0;
let payments = [];

document.addEventListener('DOMContentLoaded', function () {
    const dataEl = document.getElementById('paymentData');
    if (dataEl) {
        TOTAL = parseFloat(dataEl.getAttribute('data-total')) || 0;
        ORDER_ID = parseInt(dataEl.getAttribute('data-order-id')) || 0;
        TABLE_NUMBER = parseInt(dataEl.getAttribute('data-table-number')) || 0;

        const idsRaw = dataEl.getAttribute('data-order-ids') || '';
        ORDER_IDS = idsRaw.split(',').map(Number).filter(n => n > 0);
        if (ORDER_IDS.length === 0) ORDER_IDS = [ORDER_ID];
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
    document.getElementById('feedbackInput').closest('.payment-form-group').style.display = on ? 'none' : 'block';
    document.getElementById('paymentMethod').closest('.payment-method-group').style.display = on ? 'none' : 'block';
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
    renderSplitFeedbackFields(n);
    calcStatus();
}

function renderSplitFeedbackFields(n) {
    const container = document.getElementById('splitFeedbackFields');
    container.innerHTML = '';

    for (let i = 1; i <= n; i++) {
        container.innerHTML += `
            <div class="mb-3 border-top pt-2">
                <p class="small fw-bold mb-1">Persoon ${i}</p>
                <label class="form-label small text-muted">Betaalmethode:</label>
                <select class="form-select form-select-sm split-method mb-2" data-person="${i}">
                    <option value="Card">Card</option>
                    <option value="Cash">Cash</option>
                </select>
                <label class="form-label small text-muted">Feedback (optioneel):</label>
                <input type="text" class="form-control form-control-sm split-feedback"
                       data-person="${i}" placeholder="e.g. Great service!" maxlength="255" />
            </div>`;
    }
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
    const isSplit = document.getElementById('splitCheck').checked;

    if (isSplit) {
        const feedbackFields = document.querySelectorAll('.split-feedback');
        const methodFields = document.querySelectorAll('.split-method');
        const n = feedbackFields.length || 1;
        const perPersonAmount = parseFloat((amount / n).toFixed(2));
        const perPersonTip = parseFloat((tip / n).toFixed(2));

        feedbackFields.forEach((field, i) => {
            payments.push({
                amount: perPersonAmount,
                method: methodFields[i] ? methodFields[i].value : 'Card',
                tip: perPersonTip,
                feedback: field.value || ''
            });
        });

        document.getElementById('splitFeedbackFields').innerHTML = '';
    } else {
        const feedback = document.getElementById('feedbackInput').value || '';
        payments.push({ amount: parseFloat(amount.toFixed(2)), method, tip, feedback });
    }

    renderPayments();

    document.getElementById('amountInput').value = '';
    document.getElementById('feedbackInput').value = '';
    document.getElementById('splitCheck').checked = false;
    document.getElementById('splitSection').style.display = 'none';
    document.getElementById('feedbackInput').closest('.payment-form-group').style.display = 'block';
    document.getElementById('tipRow').style.display = 'none';
    calcStatus();
}

function renderPayments() {
    const container = document.getElementById('paymentsContainer');
    const list = document.getElementById('paymentsList');
    list.style.display = payments.length > 0 ? 'block' : 'none';

    container.innerHTML = payments.map((p, i) =>
        `<div class="payment-added-row">
            <span>Persoon ${i + 1} – ${p.method}${p.tip > 0.001
            ? ' <span class="badge bg-success ms-1">tip ' + fmt(p.tip) + '</span>'
            : ''}${p.feedback
                ? ' <span class="text-muted small ms-1">"' + p.feedback + '"</span>'
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

    let sub = fmt(totalPaid);
    if (totalTip > 0.001) sub += ' (tip: ' + fmt(totalTip) + ')';
    document.getElementById('confirmSub').textContent = sub;

    let details =
        `<div class="confirm-detail-row">
            <span>Table</span><strong>${TABLE_NUMBER}</strong>
        </div>
        <div class="confirm-detail-row">
            <span>Orders</span><strong>${ORDER_IDS.map(id => '#' + id).join(', ')}</strong>
        </div><hr/>`;

    payments.forEach((p, i) => {
        details +=
            `<div class="confirm-detail-row">
                <span>Persoon ${i + 1} – ${p.method}</span>
                <strong>${fmt(p.amount)}</strong>
            </div>`;
        if (p.feedback) {
            details += `<div style="font-size:12px;font-style:italic;color:#6b7280;margin-bottom:4px;">
                "${p.feedback}"
            </div>`;
        }
    });

    if (totalTip > 0.001) {
        details +=
            `<hr/>
            <div class="confirm-detail-row">
                <span>Tip</span>
                <strong class="text-success">${fmt(totalTip)}</strong>
            </div>`;
    }

    document.getElementById('confirmDetails').innerHTML = details;
    document.getElementById('checkoutSection').style.display = 'none';
    document.getElementById('confirmSection').style.display = 'block';

    saveAllPayments();
}
//loopt door alle orderids en stuurt voor alles een fetch.
function saveAllPayments() {
    const totalTip = payments.reduce((s, p) => s + (p.tip || 0), 0);
    const allFeedback = payments.map(p => p.feedback).filter(f => f).join(', ');

    const savePromises = ORDER_IDS.map(orderId =>
        fetch('/Payment/SavePayment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                orderId: orderId,
                tableNumber: TABLE_NUMBER,
                tipAmount: totalTip / ORDER_IDS.length,
                feedback: allFeedback || ''
            })
        }).then(r => r.json())
    );
    //alles moet gelukt zijn, als er iets mislukt is geeft het failed.
    Promise.all(savePromises)
        .then(results => {
            const failed = results.filter(r => !r.success);
            if (failed.length > 0) {
                console.error('Some payments failed:', failed);
            } else {
                console.log('All payments saved successfully');
            }
        })
        .catch(error => console.error('Error saving payments:', error));
}