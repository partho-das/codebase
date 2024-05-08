import os
import pathlib
import unittest

from selenium import webdriver

# Finds the Uniform Resource Identifier of a file
def file_uri(filename):
    return pathlib.Path(os.path.abspath(filename)).as_uri()

# Set up the web driver using Google Chrome
driver = webdriver.Chrome()
uri = file_uri("count.html")
driver.get(uri)
# Now you can use the driver to find elements by ID
element = driver.find_element_by_id("increment")
