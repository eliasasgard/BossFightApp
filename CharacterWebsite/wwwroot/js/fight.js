const urls = {
    state: '@Url.Action("State",      new { id = Model.Player.Id })',
    attack: '@Url.Action("Attack",     new { id = Model.Player.Id })',
    bossAttack: '@Url.Action("BossAttack", new { id = Model.Player.Id })',
    defend: '@Url.Action("Defend",     new { id = Model.Player.Id })'
};


const images = {
    normal: '@Url.Content("~/images/TheBoss.png")',
    bossDied: '@Url.Content("~/images/TheBoss-YouWon.png")',
    playerDied: '@Url.Content("~/images/TheBoss-YouDied.png")'
};

const element = id => document.getElementById(id);

// The page starts from the state the server rendered. Json.Serialize escapes < and >,
// so a character name cannot break out of this script tag.
let state = @Json.Serialize(Model);
let shownImage = '';
let seq = 0;        // numbers every request, so a slow, older response
let rendered = 0;   // never overwrites a newer one that is already on screen

function setBar(who, hp, max) {
    const shown = Math.max(0, hp);
    element(who + 'Bar').style.width = (max > 0 ? 100 * shown / max : 0) + '%';
    element(who + 'Hp').textContent = shown + ' / ' + max;
}

function render(s) {
    state = s;
    setBar('boss', s.boss.currentHp, s.boss.maxHp);
    setBar('player', s.player.currentHp, s.player.maxHp);

    const image = s.bossDead ? images.bossDied : s.playerDead ? images.playerDied : images.normal;
    if (image !== shownImage) { element('bossImage').src = image; shownImage = image; }

    const title = element('bannerTitle');
    if (s.playerDead) {
        title.textContent = 'YOU DIED';
        title.className = 'banner banner-died';
        element('bannerText').textContent = s.player.name + ' has fallen and cannot fight again.';
    } else if (s.bossDead) {
        title.textContent = 'VICTORY';
        title.className = 'banner banner-won';
        element('bannerText').textContent = s.boss.name + ' has been defeated!';
    }
    element('banner').hidden = !s.isOver;
    element('buttons').hidden = s.isOver;
    element('guardNote').hidden = !s.guarding || s.isOver;

    // textContent, not innerHTML, because character names are user input.
    element('log').replaceChildren(...s.log.map(e => {
        const li = document.createElement('li');
        li.className = 'list-group-item';

        const time = document.createElement('small');
        time.className = 'text-muted me-2';
        time.textContent = new Date(e.timeChanged).toLocaleTimeString();

        const text = document.createElement('span');
        text.textContent = e.description || (e.field + ' changed from ' + e.oldValue + ' to ' + e.newValue);

        li.append(time, text);
        return li;
    }));
}

async function call(url, method) {
    const mine = ++seq;
    try {
        const res = await fetch(url, { method });
        if (!res.ok) return null;
        const s = await res.json();
        if (mine > rendered) { rendered = mine; render(s); }
        return s;
    } catch {
        return null;    // network hiccup: the next refresh brings the page back in sync
    }
}

element('attackBtn').addEventListener('click', async () => {
    const attackBtn = element('attackBtn');
    attackBtn.disabled = true;                           // one second until the next attack

    const s = await call(urls.attack, 'POST');           // you hit the boss...

    setTimeout(() => {
        attackBtn.disabled = state.isOver;
        if (s && !s.isOver) call(urls.bossAttack, 'POST');   // ...and a second later he hits back
    }, 1000);
});

element('defendBtn').addEventListener('click', () => call(urls.defend, 'POST'));

// Look for damage dealt by other players, without reloading the page.
setInterval(() => { if (!state.isOver) call(urls.state, 'GET'); }, 3000);

render(state);