"""Original procedural BGM for the two mini-games. Seamless loops (circular rendering)."""
import numpy as np, wave, sys, os

SR = 44100
rng = np.random.default_rng(7)

def midi(n): return 440.0 * 2 ** ((n - 69) / 12)

class Track:
    def __init__(self, seconds):
        self.L = int(seconds * SR)
        self.buf = np.zeros((2, self.L))
    def add(self, sig, start_s, pan=0.0, gain=1.0):
        s = int(start_s * SR)
        idx = (np.arange(len(sig)) + s) % self.L
        l = np.cos((pan + 1) * np.pi / 4); r = np.sin((pan + 1) * np.pi / 4)
        np.add.at(self.buf[0], idx, sig * gain * l)
        np.add.at(self.buf[1], idx, sig * gain * r)

def env_ar(n, a, r_tau):
    t = np.arange(n) / SR
    e = np.exp(-t / r_tau)
    na = max(1, int(a * SR)); e[:na] *= np.linspace(0, 1, na)
    return e

def epiano(f, dur):
    n = int((dur + 2.0) * SR); t = np.arange(n) / SR
    s = np.zeros(n)
    for k, a in zip([1, 2, 3, 4], [1, .30, .10, .04]):
        s += a * np.sin(2 * np.pi * f * k * t) * np.exp(-t * (1.2 + 1.3 * k))
    s *= env_ar(n, 0.006, 10)
    # damp after note length
    off = int(dur * SR); s[off:] *= np.exp(-np.arange(n - off) / SR / 0.35)
    return s

def bell(f, dur=3.0):
    n = int(dur * SR); t = np.arange(n) / SR
    I = 2.2 * np.exp(-t * 3.0)
    s = np.sin(2 * np.pi * f * t + I * np.sin(2 * np.pi * f * 3.5 * t))
    s += 0.25 * np.sin(2 * np.pi * f * 2.0 * t) * np.exp(-t * 4)
    return s * env_ar(n, 0.002, 0.9)

def pad(f, dur, att=1.2, rel=1.8, bright=5):
    n = int((dur + rel) * SR); t = np.arange(n) / SR
    s = np.zeros(n)
    for det in (-0.12, 0.0, 0.11):
        ff = f * 2 ** (det / 12)
        ph = rng.uniform(0, 2 * np.pi)
        for k in range(1, bright + 1):
            s += np.sin(2 * np.pi * ff * k * t + ph * k) / k ** 1.6
    lfo = 1 + 0.15 * np.sin(2 * np.pi * 0.25 * t)
    e = np.ones(n)
    na = int(att * SR); e[:na] = np.linspace(0, 1, na) ** 2
    off = int(dur * SR); e[off:] = np.exp(-np.arange(n - off) / SR / (rel / 3))
    return s * e * lfo / 3

def bass(f, dur):
    n = int((dur + 0.5) * SR); t = np.arange(n) / SR
    s = np.sin(2 * np.pi * f * t) + 0.15 * np.sin(2 * np.pi * 2 * f * t)
    e = np.ones(n); na = int(0.05 * SR); e[:na] = np.linspace(0, 1, na)
    off = int(dur * SR); e[off:] = np.exp(-np.arange(n - off) / SR / 0.12)
    return s * e

def reverb(buf, seconds=2.5, mix=0.3):
    L = buf.shape[1]
    out = np.empty_like(buf)
    for ch in range(2):
        n = int(seconds * SR)
        ir = rng.standard_normal(n) * np.exp(-np.arange(n) / SR / (seconds / 6.9))
        ir[:int(0.012 * SR)] = 0
        ir /= np.sqrt(np.sum(ir ** 2))
        irp = np.zeros(L); irp[:n] = ir
        wet = np.fft.irfft(np.fft.rfft(buf[ch]) * np.fft.rfft(irp), L)  # circular -> seamless
        out[ch] = buf[ch] * (1 - mix) + wet * mix * 1.6
    return out

def delay(buf, sec, fb=0.4, taps=4, cross=True):
    d = int(sec * SR); out = buf.copy()
    for i in range(1, taps + 1):
        src = buf[::-1] if (cross and i % 2) else buf
        out += np.roll(src, d * i, axis=1) * fb ** i
    return out

def lowpass(buf, cutoff):
    L = buf.shape[1]; F = np.fft.rfft(buf, axis=1); fr = np.fft.rfftfreq(L, 1 / SR)
    F *= 1 / np.sqrt(1 + (fr / cutoff) ** 4)
    F *= 1 / np.sqrt(1 + (70.0 / np.maximum(fr, 1e-3)) ** 4)  # high-pass 70 Hz
    return np.fft.irfft(F, L, axis=1)

def finish(buf, peak=0.55):
    buf = buf - buf.mean(axis=1, keepdims=True)
    buf = np.tanh(buf / np.max(np.abs(buf)) * 1.2) / np.tanh(1.2)
    return buf * peak

def write(path, buf):
    data = (np.clip(buf.T, -1, 1) * 32767).astype('<i2')
    with wave.open(path, 'wb') as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR); w.writeframes(data.tobytes())

# ---------------- Sia: calm, warm (F major, 76 BPM) ----------------
def sia():
    bpm = 76; beat = 60 / bpm; bar = 4 * beat
    # chord = (bass midi, voicing midi list)
    prog = [(41, [53, 57, 60, 64]),   # Fmaj7
            (38, [50, 53, 57, 60]),   # Dm7
            (34, [50, 53, 57, 58]),   # Bbmaj7 (D F A Bb)
            (36, [48, 53, 55, 60])]   # Csus4
    prog2 = [(41, [53, 57, 60, 64]), (45, [52, 57, 60, 64]),  # F, Am7
             (34, [50, 53, 57, 62]), (36, [48, 52, 55, 60])]  # Bb(add9), C
    bars = 16
    tr = Track(bars * bar)
    seq = (prog + prog2) * 1
    arp_pat = [0, 1, 2, 3, 2, 1, 3, 2]
    for b in range(bars):
        chord = seq[(b // 2) % len(seq)]
        t0 = b * bar
        if b % 2 == 0:
            tr.add(bass(midi(chord[0] + 12), 2 * bar - 0.1), t0, 0, 0.10)
            tr.add(pad(midi(chord[1][0]), 2 * bar, bright=3), t0, -0.3, 0.06)
            tr.add(pad(midi(chord[1][2]), 2 * bar, bright=3), t0, 0.3, 0.06)
        for i, k in enumerate(arp_pat):
            n = chord[1][k] + 12
            vel = 0.20 if i % 2 == 0 else 0.14
            tr.add(epiano(midi(n), beat * 0.5), t0 + i * beat * 0.5 + rng.uniform(0, 0.008),
                   pan=0.25 * np.sin(i), gain=vel)
    # melody in second half (F major pentatonic), (bar, beat, midi, beats)
    mel = [(8, 0, 72, 1.5), (8, 1.5, 74, .5), (8, 2, 72, 2), (9, 0, 69, 2), (9, 2, 67, 2),
           (10, 0, 69, 1.5), (10, 1.5, 72, .5), (10, 2, 74, 2), (11, 0, 72, 3),
           (12, 0, 77, 1.5), (12, 1.5, 76, .5), (12, 2, 74, 2), (13, 0, 72, 2), (13, 2, 69, 2),
           (14, 0, 70, 1.5), (14, 1.5, 69, .5), (14, 2, 67, 2), (15, 0, 65, 3.5)]
    for b, bt, n, ln in mel:
        tr.add(epiano(midi(n + 12), ln * beat), b * bar + bt * beat, 0.05, 0.22)
        tr.add(bell(midi(n + 12), 2.0), b * bar + bt * beat, -0.1, 0.03)
    out = lowpass(reverb(tr.buf, 2.2, 0.28), 6500)
    return finish(out, 0.5)

# ---------------- Lucid: dreamy, floating (D minor / lydian colour, 64 BPM) ----------------
def lucid():
    bpm = 64; beat = 60 / bpm; bar = 4 * beat
    prog = [(38, [50, 57, 64, 65]),   # Dm(add9)
            (34, [46, 53, 57, 64]),   # Bbmaj7(#11-ish, E)
            (43, [55, 58, 62, 69]),   # Gm(add9)
            (45, [52, 57, 59, 64])]   # Asus2/4 (A B E)
    bars = 16
    tr = Track(bars * bar)
    for b in range(bars):
        chord = prog[(b // 2) % 4]
        t0 = b * bar
        if b % 2 == 0:
            tr.add(bass(midi(chord[0] + 12), 2 * bar - 0.2), t0, 0, 0.08)
            for j, n in enumerate(chord[1]):
                tr.add(pad(midi(n), 2 * bar, att=2.0, rel=2.5, bright=6), t0, -0.6 + 0.4 * j, 0.05)
        # music-box bells: triplet 8ths, sparse, chord tones 2 octaves up
        tones = sorted(chord[1])
        steps = 12
        for i in range(steps):
            if rng.random() < (0.55 if b >= 4 else 0.3):
                n = tones[(i * 3 + b) % 4] + 24 + (12 if rng.random() < 0.2 else 0)
                tr.add(bell(midi(n), 2.5), t0 + i * beat / 3, pan=rng.uniform(-0.7, 0.7),
                       gain=rng.uniform(0.05, 0.09))
    # slow floating lead (bars 8-15)
    mel = [(8, 0, 81, 3), (9, 0, 79, 1), (9, 1, 77, 3), (10, 0, 76, 2), (10, 2, 77, 2),
           (11, 0, 74, 4), (12, 0, 81, 2), (12, 2, 84, 2), (13, 0, 83, 3), (14, 0, 79, 2),
           (14, 2, 77, 2), (15, 0, 76, 4)]
    for b, bt, n, ln in mel:
        s = pad(midi(n), ln * beat, att=0.4, rel=1.5, bright=2)
        tr.add(s, b * bar + bt * beat, 0.0, 0.08)
        tr.add(bell(midi(n), 3.0), b * bar + bt * beat, 0.2, 0.05)
    buf = delay(tr.buf, beat * 0.75, fb=0.35, taps=4)
    out = lowpass(reverb(buf, 4.0, 0.5), 7000)
    return finish(out, 0.5)

if __name__ == '__main__':
    od = sys.argv[1]
    write(os.path.join(od, 'bgm_sia.wav'), sia())
    write(os.path.join(od, 'bgm_lucid.wav'), lucid())
    print('ok')
