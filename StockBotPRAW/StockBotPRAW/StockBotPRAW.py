import praw
from textblob import TextBlob
import pandas as pd
import re

# Set up Reddit API access using script authentication (OAuth2.0)
reddit = praw.Reddit(
    client_id='',
    client_secret='',
    user_agent=''
)

# Function to get subreddit posts
def get_posts(subreddit_name, limit=100):
    """
    Fetches the latest posts from a given subreddit.

    Args:
    - subreddit_name: Name of the subreddit to fetch posts from.
    - limit: Number of posts to fetch (default is 100).

    Returns:
    - List of Post objects from Reddit.
    """
    subreddit = reddit.subreddit(subreddit_name)
    return list(subreddit.new(limit=limit))

# Function to perform sentiment analysis and classify sentiment
def analyze_sentiment(text):
    """
    Analyzes the sentiment of a given text using TextBlob and classifies it.

    Args:
    - text: The text to analyze.

    Returns:
    - Tuple containing the sentiment polarity score and its classification.
    """
    analysis = TextBlob(text)
    sentiment_score = analysis.sentiment.polarity
    
    if sentiment_score > 1:
        sentiment_class = "potential buy"
    elif sentiment_score > 0:
        sentiment_class = "neutral"
    else:
        sentiment_class = "avoid"
    
    return sentiment_score, sentiment_class

# Function to extract stock tickers
def extract_tickers(text):
    """
    Extracts stock tickers from a given text using a regex pattern.

    Args:
    - text: The text from which to extract tickers.

    Returns:
    - List of unique stock tickers.
    """
    ticker_pattern = re.compile(r'\b(?!IPO|AI|CPI|LLC|SEC|CEO|TO|PM|MUSK|ATH|PPI|ER|AM)\$?[A-Z]{2,5}\b')
    matches = ticker_pattern.findall(text)
    return list(set(matches))  # Remove duplicates

# Get posts from the subreddits
pennystocks_posts = get_posts('pennystocks', limit=100)
daytrading_posts = get_posts('daytrading', limit=100)

# Prepare data for DataFrame
data = []

# Process posts for r/pennystocks
for post in pennystocks_posts:
    text = post.title + ' ' + post.selftext
    sentiment_score, sentiment_class = analyze_sentiment(text)
    tickers = extract_tickers(text)
    data.append({
        'Subreddit': 'pennystocks',
        'Post ID': post.id,
        'Title': post.title,
        #'Text': post.selftext,
        'Sentiment Score': sentiment_score,
        'Sentiment Classification': sentiment_class,
        'Tickers': ', '.join(tickers)  # Join tickers with comma for readability
    })

# Process posts for r/daytrading
for post in daytrading_posts:
    text = post.title + ' ' + post.selftext
    sentiment_score, sentiment_class = analyze_sentiment(text)
    tickers = extract_tickers(text)
    data.append({
        'Subreddit': 'daytrading',
        'Post ID': post.id,
        'Title': post.title,
        #'Text': post.selftext,
        'Sentiment Score': sentiment_score,
        'Sentiment Classification': sentiment_class,
        'Tickers': ', '.join(tickers)  # Join tickers with comma for readability
    })

# Create a DataFrame to store results
df = pd.DataFrame(data)

# Print or save the DataFrame
print(df.head())  # Print the first few rows for verification
df.to_csv('subreddit_sentiments_and_tickers.csv', index=False)