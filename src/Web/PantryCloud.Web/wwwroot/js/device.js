// Provides a short browser/device description for session display (e.g. "Chrome on Windows").
window.getBrowserDescription = function () {
    const ua = navigator.userAgent;
    let browser = 'Browser';
    if (ua.indexOf('Edg/') >= 0) browser = 'Edge';
    else if (ua.indexOf('Chrome') >= 0 && ua.indexOf('Edg') < 0) browser = 'Chrome';
    else if (ua.indexOf('Firefox') >= 0) browser = 'Firefox';
    else if (ua.indexOf('Safari') >= 0 && ua.indexOf('Chrome') < 0) browser = 'Safari';
    else if (ua.indexOf('Opera') >= 0 || ua.indexOf('OPR') >= 0) browser = 'Opera';
    let os = 'Device';
    if (ua.indexOf('Win') >= 0) os = 'Windows';
    else if (ua.indexOf('Mac') >= 0) os = 'Mac';
    else if (ua.indexOf('Linux') >= 0) os = 'Linux';
    else if (ua.indexOf('Android') >= 0) os = 'Android';
    else if (ua.indexOf('iPhone') >= 0 || ua.indexOf('iPad') >= 0) os = 'iOS';
    return browser + ' on ' + os;
};
