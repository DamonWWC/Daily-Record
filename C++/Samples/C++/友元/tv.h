#pragma once
class Tv;
class Remote
{
private:
    int mode;

public:
    Remote(int m = Tv::TV) : mode(m) {}
    bool voldown(Tv &t);
    bool volup(Tv &t);
    void onoff(Tv &t);
    void chanup(Tv &t);
    void chandown(Tv &t);
    void set_chan(Tv &t, int c);
    void set_mode(Tv &t);
    void set_input(Tv &t);
};
class Tv
{
public:
    // friend class Remote;
    enum
    {
        Off,
        On
    };
    enum
    {
        MinVal,
        MaxVal = 20
    };
    enum
    {
        Antenna,
        Cable
    };
    enum
    {
        TV,
        DVD
    };

    Tv(int s = Off, int mc = 0) : state(s), volume(5), maxchannel(mc), channel(2), mode(Cable), input(TV) {}
    void onoff() { state = (state == On) ? Off : On; }
    bool ison() const { return state == On; }
    bool volup();
    bool voldown();
    void chanup();
    void chandown();
    void set_mode() { mode = (mode == Antenna) ? Cable : Antenna; }
    void set_input() { input = (input == TV) ? DVD : TV; }
    void settings() const;

    friend void Remote::set_chan(Tv &t, int c);

private:
    int state;
    int volume;
    int maxchannel;
    int channel;
    int mode;
    int input;
};
inline bool Remote::voldown(Tv &t) { return t.voldown(); }
inline bool Remote::volup(Tv &t) { return t.volup(); }
inline void Remote::onoff(Tv &t) { t.onoff(); }
inline void Remote::chanup(Tv &t) { t.chanup(); }
inline void Remote::chandown(Tv &t) { t.chandown(); }
inline void Remote::set_chan(Tv &t, int c) { t.channel = c; }
inline void Remote::set_mode(Tv &t) { t.set_mode(); }
inline void Remote::set_input(Tv &t) { t.set_input(); }