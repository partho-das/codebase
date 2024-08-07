from django.shortcuts import render
from django.http import HttpResponseRedirect, HttpResponse
from django.urls import reverse
from django.core.exceptions import ObjectDoesNotExist

from .models import Flight, Passenger
# Create your views here.

def index(request):
    return render(request, "flights/index.html", {
        "flights": Flight.objects.all()
    })
  
def flight(request, flight_id):
    try:
        flight = Flight.objects.get(pk = flight_id)
    except ObjectDoesNotExist:
        return HttpResponse("404 Page Not Found!", status=404)
    passengers = flight.passengers.all()
    non_passengers = Passenger.objects.exclude(flight = flight).all()
    return render(request, "flights/flight.html",{
    "flight" : flight,
    "passengers": passengers,
    "non_passengers": non_passengers
    })

def book(request, flight_id):
     if request.method ==  "POST":
        flight =  Flight.objects.get(pk=flight_id)
        passenger = Passenger.objects.get(pk = int(request.POST["passenger"]))
        passenger.flight.add(flight)
        return HttpResponseRedirect(reverse("flight", args = (flight_id,)))
