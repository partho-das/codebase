from django.http import HttpResponse
from django.shortcuts import render

# Create your views here.

def index(request):
    # print(request)
    return HttpResponse("Hello!")

def brain(request):
    return HttpResponse(f"Hello, Brain Obrain! {request}")

def greet(request, name):
    return HttpResponse(f"Hello, {name.capitalize()}!")
def greetHtml(request, name):
    return render(request, "hello/greet.html", {
        "name": name.capitalize()
    })
