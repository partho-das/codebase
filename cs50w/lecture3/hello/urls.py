from django.urls import path

from . import views
urlpatterns = [
    path("", views.index, name="index"),
    path("<str:name>", views.greetHtml, name = "greet"),
    path("brain", views.brain, name = "brain"),
]