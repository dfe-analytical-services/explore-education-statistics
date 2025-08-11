FROM ubuntu:24.04

ENV NVM_DIR=/usr/local/nvm
ENV NODE_VERSION=22.18.0
ENV RUBY_VERSION=3.2.0
ENV NODE_PATH=$NVM_DIR/v$NODE_VERSION/lib/node_modules
ENV PATH=$NVM_DIR/versions/node/v$NODE_VERSION/bin:/home/ubuntu/.local/share/gem/ruby/$RUBY_VERSION/bin:$PATH

# Install dependencies for installing packages and developing with Ruby. 
RUN apt update && \
    apt install -y gpg curl \
    build-essential zlib1g-dev libssl-dev libyaml-dev \
    ruby-dev && \
    apt clean

# Install NVM and Node. 
RUN mkdir /usr/local/nvm && \
    curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash && \
    . $NVM_DIR/nvm.sh && \
    nvm install v$NODE_VERSION

# Create a temporary staging area to run `bundle install` during the Docker image build.
# This will be based upon the current state of `Gemfile` and `Gemfile.lock`.
# This allows us to cache all the required Gem dependencies in the Docker image itself
# so that subsequent image builds and container creations will have the Gems already cached
# in the image, thus allowing much faster doc builds and hosting.
RUN mkdir /tmp/gemfile-cache
COPY Gemfile Gemfile.lock /tmp/gemfile-cache/
RUN chown -R ubuntu:ubuntu /tmp/gemfile-cache

# Use the default `ubuntu` user (which maps to the same user as on the host machine,
# thus allowing appropriate write access to the build folders).
USER ubuntu
WORKDIR /home/ubuntu/docs

# Cache the appropriate Gems in the Docker image based on the current state of the 
# `Gemfile` and `Gemfile.lock` that was copied to a temporary staging area in a
# previous step above.
RUN gem install bundler -v 2.5.22 --user-install && \
    cd /tmp/gemfile-cache && \
    bundle config set --local path '/home/ubuntu/.local/share/gem' && \
    bundle install
