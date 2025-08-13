-- Matchmaking Lua Script for Redis
-- This script handles player matchmaking logic

-- KEYS[1] = queue_key (e.g., "matchmaking:queue")
-- ARGV[1] = player_id
-- ARGV[2] = player_mmr
-- ARGV[3] = max_wait_time (seconds)

local queue_key = KEYS[1]
local player_id = ARGV[1]
local player_mmr = tonumber(ARGV[2])
local max_wait_time = tonumber(ARGV[3])

-- Add player to queue with timestamp
redis.call('ZADD', queue_key, redis.call('TIME')[1], player_id)

-- Find potential matches within MMR range
local mmr_range = 100 -- Adjustable MMR range for matching
local min_mmr = player_mmr - mmr_range
local max_mmr = player_mmr + mmr_range

-- Get all players in queue
local queue_players = redis.call('ZRANGE', queue_key, 0, -1, 'WITHSCORES')

-- Find best match
local best_match = nil
local best_mmr_diff = math.huge

for i = 1, #queue_players, 2 do
    local candidate_id = queue_players[i]
    local candidate_time = tonumber(queue_players[i + 1])
    
    -- Skip self
    if candidate_id ~= player_id then
        -- Get candidate MMR (simplified - in real implementation, this would be stored)
        local candidate_mmr = player_mmr -- Placeholder - should get from player data
        
        -- Check if within MMR range
        if candidate_mmr >= min_mmr and candidate_mmr <= max_mmr then
            local mmr_diff = math.abs(candidate_mmr - player_mmr)
            
            -- Prefer closer MMR matches
            if mmr_diff < best_mmr_diff then
                best_match = candidate_id
                best_mmr_diff = mmr_diff
            end
        end
    end
end

-- If found match, remove both players from queue and return match
if best_match then
    redis.call('ZREM', queue_key, player_id)
    redis.call('ZREM', queue_key, best_match)
    return {player_id, best_match}
end

-- No match found, return nil
return nil
