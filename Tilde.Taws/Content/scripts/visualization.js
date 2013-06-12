$(function () {
    // determine for which type to do visualization
    var type = $('article').hasClass('xliff') ? 'xliff' : 'html5';

    // apply the appropriate visualization
    if (type == 'xliff') {
        xliff();
    } else if (type = 'html5') {
        html5();
    }
});

function html5() {
    // the output document resides in a <pre> and is encoded
    // load the escaped html of the document from <pre> and decode it
    var html = $("<noneexisting/>").html($('#document pre').html()).text();

    // inject a stylesheet and <base> in the html
    html = html.replace('<head>', '<head>' + '<base href="' + $('#document').attr('data-path') + '">');

    // remove the document which will be replaced with an iframe
    $('#document pre').remove();

    // create an iframe with the documnet
    var iframe = create_iframe(html, $('#document'), function (iframe) {
        var height = iframe.contents().height();
        iframe.css({ height: height });
        $('#output').css({ height: height + 80 });
        iframe.attr('scrolling', 'no');

        var container = $('<div/>').hide();
        $('body').append(container);

        // highlight terms
        iframe.contents()
              .find('span[its-term="yes"]')
              .each(function () {
                var clone = $(this).clone();
                container.append(clone);

                if (annotatorsRef(this) != 'http://tilde.com/term-annotation-service')
                    clone.addClass('non-tilde');

                var styles = css(clone);
                $(this).css(styles);

                clone.remove();
              })

        // tooltip
        iframe.contents()
              .find('span[its-term="yes"]')
              .filter(function () { return annotatorsRef(this) == 'http://tilde.com/term-annotation-service'; })
              .tooltip({
                items: '*',
                track: true,
                content: tooltip,
                position: { my: 'left+' + (iframe.offset().left) + ' top+210', collision: 'none' },
                close: function (e, ui) { $('.ui-tooltip').remove(); },
                open: function (e, ui) { $('body').append(ui.tooltip); }
              });

        container.remove();
    });
}

function xliff() {
    // xliff which has escaped html entities
    var html = $('#document pre').html();
    // remove it since it will be replaced with an iframe
    $('#document pre').remove();

    // unescape tags so that it's easier to use regex
    html = html.replace(/&lt;/g, '<');
    html = html.replace(/&gt;/g, '>');
    // note: hard coded color values
    html = html.replace(/(<martif type="TBX"[^>]*>)([\s\S]*?)(<\/martif>)/mg, "<span style='color: purple'>$1$2$3</span>");
    html = html.replace(/(<mrk[^>]+mtype\s*=\s*["']*\s*term\s*["']*[^>]+>)([\s\S]*?)(<\/mrk>)/mg, "<span style='color: #fba43a'>$1</span>$2<span style='color: #fba43a'>$3</span>");
    // escape html tags back again
    html = html.replace(/</g, '&lt;');
    html = html.replace(/>/g, '&gt;');
    // unescape our own tags that were escaped in the previous step
    html = html.replace(/&lt;span style='color: ([^']+)'&gt;([\s\S]*?)&lt;\/span&gt;/gm, "<span style='color: $1'>$2</span>");

    html = '<pre>' + html + '</pre>';

    var iframe = create_iframe(html, $('#document'), function (iframe) {
        var height = iframe.contents().height() + 20;
        iframe.css({ height: height });
        $('#output').css({ height: height + 80 });
        iframe.attr('scrolling', 'auto');
    });
}

function create_iframe(html, parent, load) {
    var iframe = $('<iframe seamless="seamless" src="about:blank"/>');
    parent.append(iframe);

    iframe.load(function () { load(iframe); });

    var iframeDoc = iframe[0].contentDocument || iframe[0].contentWindow.document;
    iframeDoc.open();
    iframeDoc.write(html);
    iframeDoc.close();

    return iframe;
}

function tooltip() {
    var iframe = $(this.ownerDocument);

    var ref = $(this).attr('its-term-info-ref');
    var confidence = $(this).attr('its-term-confidence');
    //var tbx = $(iframe.find(ref).html());
    var xml = iframe.find(ref).html() || '';
    xml = xml.replace(/<(!|\?).*?>/g, '').trim();
    var tbx = $($.parseHTML(xml));

    var tooltip = $('<div/>');
    tooltip.append($('<div class="annotator">Annotated by: ' + (ref ? 'Term bank based terminology annotation method' : 'Statistical terminology annotation method') + '</div>'));
    if (confidence !== undefined) tooltip.append($('<div class="confidence">Confidence: <span>' + confidence + '</span></div>'));

    if (tbx && tbx.length) {
        tooltip.addClass('tbx');
        tbx.find('langSet').map(function () {
            var langset = $('<ul class="langset"/>');
            langset.append($('<li class="lang"/>').text($(this).attr('xml:lang')));
            $(this).find('ntig').each(function () {
                var ntig = $('<ul/>');
                ntig.append($('<li class="term"/>').text($(this).find('term').text()));
                $(this).find('xref').each(function () {
                    var xref = $('<li class="xref"/>').text($(this).text());
                    ntig.append(xref);
                });
                langset.append($('<li class="ntig"/>').append(ntig));
            });
            tooltip.append(langset);
        });
    }

    return tooltip;
}

function annotatorsRef(element) {
    while ($(element).get(0)) {
        var iri = terminology(element);
        if (iri) {
            return iri;
        }
        element = $(element).parent();
    }

    function terminology(element) {
        var annRefs = $(element).attr('its-annotators-ref');
        
        // no attribute, iri is undefined
        if (!annRefs)
            return undefined;

        // find the terminology data category and return the iri
        return annRefs
            .split(' ')
            .map(function(a) { return a.split('|'); })
            .filter(function (a) { return a[0] == 'terminology'; })
            .map(function(a) { return a[1]; })
            .pop();
    }
}

// http://stackoverflow.com/questions/754607/can-jquery-get-all-css-styles-associated-with-an-element
function css(a) {
    function css2json(css) {
        var s = {};
        if (!css) return s;
        if (css instanceof CSSStyleDeclaration) css = css.cssText;
        if (typeof css == "string") {
            css = css.split(";");
            for (var i in css) {
                var l = css[i].split(": ");
                if (l[0].trim()) {
                    s[l[0].trim().toLowerCase()] = l[1].trim().replace('!important', '').replace('! important', '');
                }
            }
        }
        return s;
    }

    var sheets = document.styleSheets, o = {};
    for (var i in sheets) {
        var rules = sheets[i].rules || sheets[i].cssRules;
        for (var r in rules) {
            try {
                if (a.is(rules[r].selectorText)) {
                    o = $.extend(o, css2json(rules[r].style), css2json(a.attr('style')));
                }
            } catch (e) {
            }
        }
    }
    return o;
}
