FROM ubuntu:24.04

# Prevent interactive prompts during build
ENV DEBIAN_FRONTEND=noninteractive

# 1. Define exact versions
ENV NVM_DIR=/usr/local/nvm
ENV NODE_VERSION=22.20.0
ENV RUBY_VERSION=3.2.6
# IMPORTANT: Ruby 3.2.x still uses '3.2.0' as the ABI directory name for gems
ENV RUBY_ABI_VERSION=3.2.0

# 2. Install dependencies for compiling Ruby
RUN apt update && \
    apt install -y gpg curl build-essential zlib1g-dev libssl-dev \
    libyaml-dev libreadline-dev libffi-dev git && \
    apt clean

# 3. Install ruby-build and compile exact Ruby version
RUN git clone https://github.com/rbenv/ruby-build.git /tmp/ruby-build && \
    /tmp/ruby-build/install.sh && \
    ruby-build $RUBY_VERSION /usr/local

# 4. Install NVM and Node
RUN mkdir /usr/local/nvm && \
    curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash && \
    . $NVM_DIR/nvm.sh && \
    nvm install v$NODE_VERSION

# 5. Set up the User and Paths
# We include the specific bin folder for user-installed gems
ENV PATH=/home/ubuntu/.local/share/gem/ruby/$RUBY_ABI_VERSION/bin:$NVM_DIR/versions/node/v$NODE_VERSION/bin:$PATH

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

# 6. Install Bundler and Gems
# Cache the appropriate Gems in the Docker image based on the current state of the 
# `Gemfile` and `Gemfile.lock` that was copied to a temporary staging area in a
# previous step above.
RUN gem install bundler -v 2.5.22 --user-install && \
    cd /tmp/gemfile-cache && \
    # We use 'bundle' directly now because it's in the PATH
    bundle config set --local path '/home/ubuntu/.local/share/gem' && \
    bundle install