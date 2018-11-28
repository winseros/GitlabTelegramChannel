# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vagrant.plugins = "vagrant-disksize", "vagrant-vbguest"
  config.vm.hostname = "docker-runtime"
  config.vm.box = "debian/stretch64"
  config.disksize.size = "10GB"
  config.vm.provider "virtualbox" do |v|
    v.name = "TGramIntegration - Dev - Docker"
    v.memory = 1024
    v.cpus = 1
  end
  config.vm.synced_folder ".", "/vagrant", type: "virtualbox"
  config.vm.provision "shell", inline: <<-SHELL
    apt-get update
    apt-get install -y apt-transport-https ca-certificates curl gnupg2 software-properties-common
    curl -fsSL https://download.docker.com/linux/debian/gpg | sudo apt-key add -
    add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable"
    apt-get update
    apt-get -y install docker-ce
    addgroup vagrant docker
  SHELL
end
