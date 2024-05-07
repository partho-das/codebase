from django.shortcuts import render
from django.http import HttpResponse, Http404

# Create your views here.

def index(request):
    # return render(request, "singlepage1/index.html")
    return render(request, "singlepage1/index.html", {
        # "entries": util.list_entries()
    })

text = ["Text1", "Text2", "Text3"]

def section(request, id):
    if 1 <= id <= 3: 
        return HttpResponse(text[id - 1])
    else:
        return Http404("No such Section!")