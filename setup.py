from setuptools import setup, find_packages

with open("README.md", "r", encoding="utf-8") as fh:
    long_description = fh.read()

with open("requirements.txt", "r", encoding="utf-8") as fh:
    requirements = fh.read().splitlines()

setup(
    name="miyagi_mkdocs_material_package",
    version="0.1.0",
    author="Govind Kamtamneni",
    author_email="gok@microsoft.com",
    description="A package that uses mkdocs-material and related plugins",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/Azure-samples/intelligent-app-workshop",
    packages=find_packages(),
    classifiers=[
        "Development Status :: 3 - Alpha",
        "Intended Audience :: Developers",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.6",
        "Programming Language :: Python :: 3.7",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
    ],
    python_requires=">=3.6",
    install_requires=requirements,
)
