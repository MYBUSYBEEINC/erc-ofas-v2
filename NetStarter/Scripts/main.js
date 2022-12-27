//show back to top button at bottom right corner after the page scrolled
const topnav = document.querySelector("#top-navigation");
void 0 !== topnav && null != topnav && document.addEventListener("scroll", () => {
    window.scrollY > 99 ? topnav.classList.add("scrolled") : topnav.classList.remove("scrolled"), topnav.classList.contains("scrolled-shadow") && (window.scrollY > 100 ? topnav.classList.add("shadow") : topnav.classList.remove("shadow"))
});
const scrollTop = document.querySelector(".scroll-top");
if (void 0 !== scrollTop && null != scrollTop) {
    let d = function () {
        window.scrollY > 99 ? scrollTop.classList.add("active") : scrollTop.classList.remove("active")
    },
        g = function () {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            })
        };
    window.addEventListener("load", d), document.addEventListener("scroll", d), scrollTop.addEventListener("click", g)
}

//hide the dummy header and footer of a table after finish loading table data
function hideDummySpinnerHeaderFooter(tablewrapperid) {
    let spinner = document.querySelector("#" + tablewrapperid + " .spinner");
    let dummyFooter = document.querySelector("#" + tablewrapperid + " .dummyfooter");
    spinner.classList.remove("d-flex");
    spinner.classList.add("d-none");
    dummyFooter.classList.add("d-none");
}

//set background image of div with data-img tag
var bgimg = document.querySelector("[data-img]");
if (void 0 !== bgimg && null != bgimg)
    for (var element, dataimgs = document.querySelectorAll("[data-img]"), i = 0; element = dataimgs[i]; i++) {
        var h = element.getAttribute("data-img"),
            a = element.getAttribute("data-img-position"),
            b = element.getAttribute("data-img-attachment");
        element.style.background = "url(" + h + ")", void 0 !== a && null != a ? element.style.backgroundPosition = a : element.style.backgroundPosition = "center center", void 0 !== b && null != b ? element.style.backgroundAttachment = b : element.style.backgroundAttachment = "scroll", element.style.backgroundSize = "cover", element.style.backgroundRepeat = "no-repeat"
    }

// self executing function
(function () {
    initDropdownlist();
})();


//initialize drop down list
function initDropdownlist() {
    var selectwrapper = document.querySelector('.select-wrapper');
    if (void 0 !== selectwrapper && null != selectwrapper) {
        for (const ddl of document.querySelectorAll(".select-wrapper")) {
            ddl.addEventListener('click', function () {
                this.querySelector('.select').classList.toggle('open');
            });
        }
        window.addEventListener('click', function (e) {
            for (const select of document.querySelectorAll(".select")) {
                if (!select.contains(e.target)) {
                    select.classList.remove('open');
                }
            }
        });
    }
    var ddls = document.querySelector(".select-wrapper");
    if (void 0 !== ddls && null != ddls) {
        for (const ddl of document.querySelectorAll(".select-wrapper")) {
            let selected = ddl.querySelector('.custom-option.selected');
            if (void 0 !== selected && null != selected) {
                let hiddenInput = ddl.querySelector('.select input.custom-option');
                if (void 0 !== hiddenInput && null != hiddenInput) {
                    hiddenInput.setAttribute("value", selected.getAttribute("data-value"));
                }
            }
        }
    }
    var ddlOptions = document.querySelector(".select input.custom-option");
    if (void 0 !== ddlOptions && null != ddlOptions) {
        for (const option of document.querySelectorAll(".custom-option")) {
            option.addEventListener('click', function () {
                if (!this.classList.contains('selected')) {
                    this.parentNode.querySelector('.custom-option.selected').classList.remove('selected');
                    this.classList.add('selected');
                    this.closest('.select').querySelector('.select__trigger span').textContent = this.textContent;
                    var input = this.closest('.select').querySelector('input');
                    if (void 0 !== input && null != input) {
                        this.closest('.select').querySelector('input').value = this.getAttribute("data-value");
                    }
                }
            })
        }
    }
}

//this function will adjust the table that export to pdf to fit the width of pdf
function adjustPdfColWidth(tableIdPrefix) {
    var colCount = new Array();
    $('#' + tableIdPrefix + '-table').find('tbody tr:first-child td').each(function () {
        let col = $(this).html();
        if (col.includes("actioncol") == false) {
            if ($(this).attr('colspan')) {
                for (var i = 1; i <= $(this).attr('colspan'); $i++) {
                    colCount.push('*');
                }
            }
            else { colCount.push('*'); }
        }
    });
    return colCount;
}

//this function will exclude the action column when export to pdf/excel
function getColumnsToBeExported(tableIdPrefix) {
    var colsToBeExported = new Array();
    var count = 0;
    $('#' + tableIdPrefix + '-table').find('tbody tr:first-child td').each(function () {
        let col = $(this).html();
        if (col.includes("actioncol") == false) {
            colsToBeExported.push(count);
        }
        count++;
    });
    return colsToBeExported;
}

//for multi-select drop down list
var multichoice = document.querySelector("select.multichoice");
if (void 0 !== multichoice && null != multichoice)
    for (var element, multichoices = document.querySelectorAll("select.multichoice"), i = 0; element = multichoices[i]; i++) {
        var multiselect = new Choices(element, {
            removeItemButton: true
        });
    }

//initialize bootstrap tooltip
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
});

//Hide success message at top right corner automatically after 2500ms (2.5 second)
setTimeout(() => {
    $('#successtoast-container .toast.show').removeClass('show');
    $('#successtoast-container').hide();
}, 2500);

//Hide fail message at top right corner automatically after 4500ms (4.5 second)
setTimeout(() => {
    $('#failedtoast-container .toast.show').removeClass('show');
    $('#failedtoast-container').hide();
}, 4500);

//Convert 100,000 to 100K etc (Not used in this project, but might used in other projects)
function getNumberAbbreviation(a) {
    var e = a;
    if (a >= 1e3) {
        for (var f = ["", "k", "m", "b", "t"], c = Math.floor(("" + a).length / 3), b = "", d = 2; d >= 1 && !(((b = parseFloat((0 != c ? a / Math.pow(1e3, c) : a).toPrecision(d))) + "").replace(/[^a-zA-Z 0-9]+/g, "").length <= 2); d--);
        b % 1 != 0 && (b = b.toFixed(1)), e = b + f[c]
    }
    return e
}

//copy to clipboard (Not used in this project, but might used in other projects)
function copyToClipboard(b, c) {
    var a = document.createElement("input"),
        d = document.querySelector("#" + b).innerText;
    a.value = d, document.body.appendChild(a), a.select(), document.execCommand("copy"), document.body.removeChild(a), new bootstrap.Modal(document.getElementById(c), {}).show()
}
