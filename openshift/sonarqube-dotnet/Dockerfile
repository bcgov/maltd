FROM openshift/jenkins-slave-base-centos7

USER root

ENV SONARQUBE_SCANNER_VERSION 4.8.0
ENV HOME /home/jenkins
ENV DOT_NET_VERSION 3.1 

# Install .NET sdk rpm
RUN rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm

# Install .NET sdk
RUN yum install -y centos-release-scl-rh && \
    INSTALL_PKGS="wget dotnet-sdk-$DOT_NET_VERSION" && \
    yum install -y --setopt=tsflags=nodocs install $INSTALL_PKGS && \
    rpm -V $INSTALL_PKGS && \
    yum clean all

WORKDIR /

# Install SonarQube Scanner as the running user
RUN dotnet tool install --global dotnet-sonarscanner
ENV PATH="${PATH}:$HOME/.dotnet/tools"

RUN chown -R 1000:0 /usr/local/bin && \
    chown -R 1000:0 $HOME && \
    chmod -R g+rwx $HOME

USER 1000
