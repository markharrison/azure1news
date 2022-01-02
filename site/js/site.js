function setCookie(strCookieName, strCookieValue) {
    var myDate = new Date();
    myDate.setMonth(myDate.getMonth() + 12);
    document.cookie = strCookieName + "=" + strCookieValue + ";expires=" + myDate;
}

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length === 2) return parts.pop().split(";").shift();
}

function openNav() {

    $("#idNavOverlay").css("display", "block");
    $("#idNavSidebar").css("marginLeft", "0px");
    $("#idNavSidebar").css("marginRight", "0px");
    $("#idMainContent").css("marginLeft", "252px");
    $("#idMainContent").css("marginRight", "-248px");
}

function closeNav() {

    $("#idNavSidebar").css("marginLeft", "-250px");
    $("#idNavSidebar").css("marginRight", "250px");
    $("#idMainContent").css("marginLeft", "2px");
    $("#idMainContent").css("marginRight", "2px");
    $("#idNavOverlay").css("display", "none");
}

function toggleNav() {
    if ($("#idNavButton").hasClass("is-active"))
        closeNav();
    else
        openNav();
    $("#idNavButton").toggleClass("is-active");
}

var gColorScheme = "";

function setHash(strHash) {
    history.replaceState({}, "Azure 1", "#" + strHash);
}

function resetHash() {
    setHash("");
}

function setConfigColorScheme(strColorScheme) {
    setCookie("ColorScheme", strColorScheme);
    gColorScheme = strColorScheme;
}

function getConfigColorScheme() {
    var gColorScheme = getCookie("ColorScheme");
    if (gColorScheme === undefined) {
        gColorScheme = "dark";
        setConfigColorScheme(gColorScheme);
    }
    return gColorScheme;
}

function doColorScheme() {

    gColorScheme = getConfigColorScheme();

    if (gColorScheme === "light") {
        $("html").css("background-color", "white");
        $(".linktablearticlecell").addClass("linktablecelllight").removeClass("linktablecelldark");
        $(".icon-fa-link").addClass("icon-black_fa-link").removeClass("icon-white_fa-link");
        $(".icon-fa-videoplay").addClass("icon-black_fa-videoplay").removeClass("icon-white_fa-videoplay");
        $(".linktableheadercell").addClass("linktablecelllight").removeClass("linktablecelldark");
        $(".linktableseparatorcell").addClass("linktablecelllight").removeClass("linktablecelldark");
        $(".linktabledaycell").addClass("linktablecelllight").removeClass("linktablecelldark");
        $("hr").addClass("hrlight").removeClass("hrdark");

    }
    else {
        $("html").css("background-color", "#191919");
        $(".linktablearticlecell").addClass("linktablecelldark").removeClass("linktablecelllight");
        $(".icon-fa-link").addClass("icon-white_fa-link").removeClass("icon-black_fa-link");
        $(".icon-fa-videoplay").addClass("icon-white_fa-videoplay").removeClass("icon-black_fa-videoplay");
        $(".linktableheadercell").addClass("linktablecelldark").removeClass("linktablecelllight");
        $(".linktableseparatorcell").addClass("linktablecelldark").removeClass("linktablecelllight");
        $(".linktabledaycell").addClass("linktablecelldark").removeClass("linktablecelllight");
        $("hr").addClass("hrdark").removeClass("hrlight");

    }

}

function changeColorScheme(strColorScheme) {
    setConfigColorScheme(strColorScheme);
    doColorScheme();

    // document.location.reload();
}

function doConfig() {
    var strHTML = "";
    strHTML += "<i class='icon-black_fa-moon iconfa20xp'></i> &nbsp; <a href='#' class='modal-link' onclick='changeColorScheme(\"dark\");return false'>Set Dark scheme</a><br /><br />";
    strHTML += "<i class='icon-black_fa-sun  iconfa20xp'></i> &nbsp; <a href='#' class='modal-link' onclick='changeColorScheme(\"light\");return false'>Set Light scheme</a><br />";

    toggleNav();

    $('#idModalTitle').html("Config");
    $('#idModalBody').html(strHTML);
    $('#idModal').modal("show");

}

function doInfo() {

    var part1 = "mark.harrison";
    var part2 = "microsoft.com";
    var part3 = "?subject=azure1";
    var mailaddr = "mai" + "lto:" + part1 + '@' + part2 + part3;

    var strHTML = "";
    strHTML += "This site 'azure1.dev' was only built for my own use - to learn and demo.<br /><br />";
    strHTML += "If it's useful for others thats great - but it is provided 'as is' with no warranties, and confer no rights.<br /><br />";
    strHTML += "Feedback welcome / appreciated.<br /><br />";
    strHTML += "Mark Harrison<br /><br />";
    strHTML += "<span class='iconmodal' onclick='window.open(\"" + mailaddr + "\",\"_self\");' >";
    strHTML += "<i class='icon-black_fa-envelope iconfa20xp'></i>Email</span>";
    strHTML += "&nbsp;&nbsp;&nbsp;&nbsp;";
    strHTML += "<span class='iconmodal' onclick='window.open(\"https://www.linkedin.com/in/markharrison-uk/\",\"_blank\");' >";
    strHTML += "<i class='icon-black_fa-linkedin iconfa20xp'></i>LinkedIn</span>";
    strHTML += "&nbsp;&nbsp;&nbsp;&nbsp;";
    strHTML += "<span class='iconmodal' onclick='window.open(\"https://github.com/markharrison/\",\"_blank\");' >";
    strHTML += "<i class='icon-black_fa-github iconfa20xp'></i>Github</span>";
    strHTML += "<br /><br />";

    toggleNav();

    $('#idModalTitle').html("About Azure1");
    $('#idModalBody').html(strHTML);
    $('#idModal').modal("show");

}

function getIconFromClass(obj) {
    var ret = null;

    var classNames = $(obj).children("i").attr("class").toString().split(' ');
    $.each(classNames, function (i, className) {
        if (className.startsWith("icon-")) {
            var xclassName = className.split('_');
            ret = xclassName[1];
        }
    });
    return ret;
};

function doCallBacks() {

    $(document).keyup(function (e) {
        if (e.keyCode == 27) { // escape  
            if ($("#idNavButton").hasClass("is-active")) {
                toggleNav();
            }
        }
    });

    $('#idLightbox').on('hide.bs.modal', function (e) {
        $('#idLightboxBody').html("");
        $('#idServiceModal').show();
        setTimeout(function () { $('#idServiceModalClose').focus(); }, 500);
    });


    $(".iconsidebar").on({
        mouseover: function () {
            var icon = getIconFromClass(this);
            this.style.cursor = "pointer";
            $(this).children("i").addClass("icon-red_" + icon).removeClass("icon-white_" + icon);
            $(this).css("color", "red");
        },
        mouseout: function () {
            var icon = getIconFromClass(this);
            this.style.cursor = "default";
            $(this).children("i").addClass("icon-white_" + icon).removeClass("icon-red_" + icon);
            $(this).css("color", "white");
        }
    });

    $("#idModalBody").on({
        mouseover: function () {
            var icon = getIconFromClass(this);
            this.style.cursor = "pointer";
            $(this).children("i").addClass("icon-red_" + icon).removeClass("icon-black_" + icon);
            $(this).css("color", "red");
            $(this).css("text-decoration", "underline");
        },
        mouseout: function () {
            var icon = getIconFromClass(this);
            this.style.cursor = "default";
            $(this).children("i").addClass("icon-black_" + icon).removeClass("icon-red_" + icon);
            $(this).css("color", "black");
            $(this).css("text-decoration", "none");
        }
    }, ".iconmodal");

    $("#idMainContent").on({
        mouseover: function () {
            var icon = getIconFromClass(this);
            this.style.cursor = "pointer";
            if (gColorScheme === "light") {
                $(this).children("i").addClass("icon-white_" + icon).removeClass("icon-black_" + icon);
                $(this).removeClass("linktablecelllight").addClass("linktablecelllightselected");
            }
            else {
  //              $(this).children("i").addClass("icon-white_" + icon).removeClass("icon-white_" + icon);
                $(this).removeClass("linktablecelldark").addClass("linktablecelldarkselected");
            }
        },
        mouseout: function () {
            var icon = getIconFromClass(this);
            this.style.cursor = "default";
            if (gColorScheme === "light") {
                $(this).children("i").addClass("icon-black_" + icon).removeClass("icon-white_" + icon);
                $(this).removeClass("linktablecelllightselected").addClass("linktablecelllight");
            }
            else {
 //               $(this).children("i").addClass("icon-white_" + icon).removeClass("icon-white_" + icon);
                $(this).removeClass("linktablecelldarkselected").addClass("linktablecelldark");
            }
        }
    }, ".linktablearticlecell");

}

function doIndexReady() {

    if (window.location.hostname.toLowerCase().indexOf("azure1.io") >= 0) {
        var msg = "This site is moving to https://news.azure1.dev - update your bookmarks";
        alert(msg);
        $("body").html("<br><br>&nbsp;&nbsp;" + msg + "<br><br>&nbsp;&nbsp;<i class='iconfa15xp icon-black_fa-link'></i>&nbsp;<a href='https://news.azure1.dev'>https://news.azure1.dev</a><br><br>");
        return;
    }

    doColorScheme();
    doCallBacks();
    $("body").removeClass("preload");
    $("#idPage").show();
}
